using System;
using System.IO;
using System.Xml.Serialization;
using System.Security;
using System.Net;
using Microsoft.Win32;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using ApathyEngine.Utilities;

namespace ApathyEngine.IO
{
    /// <summary>
    /// Creates a new IOManager for save management.
    /// </summary>
    /// <typeparam name="T">The type of data to save. This is required to have a parameterless constructor
    /// that returns a default set of save data.</typeparam>
    public class IOManager<T>
        where T : new()
    {
        /// <summary>
        /// Fires when a new save is loaded.
        /// </summary>
        public event Action OnSaveChanged;

        /// <summary>
        /// Fires when a save is deleted. Passes the save slot of the save deleted.
        /// </summary>
        public event Action<int> OnSaveDeleted;

        /// <summary>
        /// Gets if the IOManager is currently saving a game.
        /// </summary>
        public bool IsSaving { get; private set; }

        /// <summary>
        /// Indicates if a save has been loaded and CurrentSave is valid.
        /// </summary>
        public bool SaveLoaded { get; private set; }

        /// <summary>
        /// Gets if the IOManager managed to successfully load from disk.
        /// </summary>
        public bool SuccessfulLoad { get; private set; }

        /// <summary>
        /// Gets the current save number; which is 1-based and not 0-based.
        /// </summary>
        public int CurrentSaveNumber { get { return currentSave + 1; } }

        /// <summary>
        /// Gets the currently loaded save.
        /// </summary>
        public T CurrentSave 
        { 
            get 
            {
                if(SaveLoaded)
                    return file.Saves[currentSave];
                throw new InvalidOperationException("No save has been set as current yet.");
            }
        }
        public WindowsOptions CurrentSaveWindowsOptions
        {
            get
            {
                return CurrentSave.Options;
            }
        }
        public XboxOptions CurrentSaveXboxOptions
        {
            get
            {
                return CurrentSave.Xoptions;
            }
        }
        protected XmlSerializer saveDataSerializer;

#if XBOX360
        private IAsyncResult result;
        private StorageContainer container;
        private StorageDevice device;
        private bool initializing;
#endif
        protected Stream currentStream;
        protected Stream backupStream;
        protected readonly string filename;
        protected readonly string backupName;
        protected SaveFile<T> file;
        protected SaveFile<T> defaultSaveData;

        protected int currentSave = -1;
        protected object lockingObject = new object();
        protected bool doNotSave = false; // if this is true we should not save under any circumstances.

        /// <summary>
        /// Creates a new IOManager.
        /// </summary>
        /// <param name="defaultSaveData">An instance of the default save data for the save file.</param>
        /// <param name="filename">Filename to save data to. Will use filename - .ext + .bak for backups.</param>
        public IOManager(SaveFile<T> defaultSaveData, string filename)
        {
#if XBOX360
            initializing = true;

            try
            {
                result = StorageDevice.BeginShowSelector(null, null);
                result.AsyncWaitHandle.WaitOne();
                device = StorageDevice.EndShowSelector(result);
                result.AsyncWaitHandle.Close();
            }
            catch
            { }

            try
            {
                result = device.BeginOpenContainer("Accelerated Delivery", null, null);
                result.AsyncWaitHandle.WaitOne();
                container = device.EndOpenContainer(result);
                result.AsyncWaitHandle.Close();
            }
            catch 
            { }
#endif
            saveDataSerializer = new XmlSerializer(typeof(SaveFile<T>));

            this.filename = filename;
            this.backupName = Path.ChangeExtension(filename, ".bak");
            this.defaultSaveData = defaultSaveData;
            file = (SaveFile<T>)defaultSaveData.Clone();

            try { Load(); SuccessfulLoad = true; }
            catch
            {
                SuccessfulLoad = false;
#if WINDOWS
                New();
#endif
            }
            finally { if(CurrentSaveWindowsOptions != null && CurrentSaveXboxOptions != null) InputManager.SetOptions(CurrentSaveWindowsOptions, CurrentSaveXboxOptions); else throw new Exception("Options are null, crash imminent."); } 
#if XBOX360
            initializing = false;
#endif
        }

        ~IOManager()
        {
            if(currentStream != null)
                currentStream.Close();
            if(backupStream != null)
                backupStream.Close();
        }

        /// <summary>
        /// Creates a new save file.
        /// </summary>
        public virtual void New()
        {
            if(doNotSave)
            {
                Log.WriteToLog(this, "New() was called and doNotSave was true. Aborting.");
                return;
            }
            
            Log.WriteToLog(this, "Attempting to create new save file.");

            file = (SaveFile<T>)defaultSaveData.Clone();
#if XBOX360
            try
            {
                int? initialResult;
                do
                {
                    initialResult = SimpleMessageBox.ShowMessageBox("No Options Found", "Please select a location to save options.",
                        new string[] { "Okay", "Continue Without Saving" }, 0, MessageBoxIcon.Alert);
                    switch(initialResult)
                    {
                        case -1:
                        case 1: returnOptionsToDefault();
                            return;
                        case 0: StorageDevice.BeginShowSelector((PlayerIndex)Program.Game.MessagePad, storageSelectEnd, null);
                            break;
                    }
                } while(initialResult == null);

                if(!container.FileExists(filename))
                    currentStream = container.CreateFile(filename);
                else
                    currentStream = container.OpenFile(filename, FileMode.Open);
                
                if(!container.FileExists(backupName))
                    backupStream = container.CreateFile(backupName);
                else
                    backupStream = container.OpenFile(backupName, FileMode.Create);

                saveDataSerializer.Serialize(backupStream, file);
                HandleEncryption(ref backupStream);
                backupStream.Seek(0, SeekOrigin.Begin);
                backupStream.CopyTo(currentStream);

                backupStream.Close();
                currentStream.Close();
                container.Dispose();
            }
            catch(Exception ex)
            {
                if(backupStream != null)
                    backupStream.Close();
                if(currentStream != null)
                    currentStream.Close();
                if(container != null)
                    container.Dispose();
                throw ex;
            }
#elif WINDOWS
            try
            {
                if(!File.Exists(filename))
                    currentStream = File.Create(filename);
                else
                {
                    currentStream = File.Open(filename, FileMode.Open);
                    Log.WriteToLog(this, "Warning: New() is overwriting a currently existing save file.");
                }
                if(!File.Exists(backupName))
                    backupStream = File.Create(backupName);
                else
                    backupStream = File.Open(backupName, FileMode.Open);

                saveDataSerializer.Serialize(backupStream, file);
                handleEncryption(ref backupStream);

                backupStream.Seek(0, SeekOrigin.Begin);
                backupStream.CopyTo(currentStream);
            }
            catch(Exception ex)
            {
                Log.WriteToLog(this, "Save file creation failed.");
                Log.WriteExceptionToLog(ex, true);
                throw ex;
            }
            finally
            {
                if(backupStream != null)
                    backupStream.Close();
                if(currentStream != null)
                    currentStream.Close();
            }
#endif
            Log.WriteToLog(this, "Save file creation successful.");
            currentSave = 0;
        }

        /// <summary>
        /// Saves the options to a file.
        /// </summary>
        public virtual void Save(bool uploadAsync)
        {
            if(doNotSave)
            {
                Log.WriteToLog(this, "Save() was called and doNotSave was true. Aborting.");
                return;
            }

            Log.WriteToLog(this, "Attempting to save game data.");
#if XBOX360
            try
            {
                if(!container.FileExists(filename) && !container.FileExists(backupName))
                    return;

                currentStream = container.CreateFile(filename);
                backupStream = container.CreateFile(backupName);

                saveDataSerializer.Serialize(backupStream, file);
                HandleEncryption(ref backupStream);
                
                backupStream.Seek(0, SeekOrigin.Begin);
                backupStream.CopyTo(currentStream);
                
                backupStream.Close();
                currentStream.Close();
                container.Dispose();
            }
            catch(Exception ex)
            { 
                if(backupStream != null)
                    backupStream.Close();
                if(currentStream != null)
                    currentStream.Close();
                if(container != null)
                    container.Dispose();
                throw ex;
            }
#elif WINDOWS
            try
            {
                if(!File.Exists(filename) && !File.Exists(backupName))
                    return;

                backupStream = File.Create(backupName);
                currentStream = File.Create(filename);

                saveDataSerializer.Serialize(backupStream, file);
                handleEncryption(ref backupStream);

                backupStream.Seek(0, SeekOrigin.Begin);
                int maxRetries = 5;
                while(tryLoad(backupStream))
                {
                    maxRetries--;
                    if(maxRetries == 0)
                        break;
                }

                if(maxRetries != 0)
                {
                    backupStream.CopyTo(currentStream);
                    currentStream.Close();
                }
                else
                    Log.WriteExceptionToLog(new InvalidDataException("Could not save file after five retries."), false);
            }
            catch(Exception ex)
            {
                Log.WriteToLog(this, "Save failed for:\n" + ex.Message);
                throw ex;
            }
            finally
            {
                if(backupStream != null)
                    backupStream.Close();
                if(currentStream != null)
                    currentStream.Close();
            }
#endif
            Log.WriteToLog(this, "Save successful.");
        }

        /// <summary>
        /// Loads the options from the XML.
        /// </summary>
        public virtual void Load()
        {
            Log.WriteToLog(this, "Attempting to load game data.");
#if XBOX360
            try
            {
                if(container == null)
                {
                    if(initializing)
                        throw new FileNotFoundException("Set Manager to null and continue along.");
                    int? devSelRes;
                    do
                    {
                        devSelRes = SimpleMessageBox.ShowMessageBox("Save", "No storage device is selected. Please select a device.",
                            new string[] { "Select a Device", "Continue Without Saving" }, 0, MessageBoxIcon.Alert);
                        switch(devSelRes)
                        {
                            case -1: // Fall through
                            case 1: returnOptionsToDefault();
                                return;
                            case 0: StorageDevice.BeginShowSelector(storageSelectEnd, null);
                                break;
                        }
                    } while(devSelRes == null);
                }
              
            try
            {
            FileNotFound:
                if(!container.FileExists(filename))
                {
                    if(!container.FileExists(backupName))
                    {
                        if(initializing)
                            throw new FileNotFoundException("Set Manager to null and continue along.");
                        int? fileResult;
                        do
                        {
                            fileResult = SimpleMessageBox.ShowMessageBox("No File Found", "No options file was found on the storage device. Would you like to create a new file or select a new device?",
                                new string[] { "Create File", "Select New Device", "Exit Without Loading" }, 0, MessageBoxIcon.Alert);
                            switch(fileResult)
                            {
                                case 0: New();
                                    return;
                                case 1: result = StorageDevice.BeginShowSelector(null, null);
                                    result.AsyncWaitHandle.WaitOne();
                                    device = StorageDevice.EndShowSelector(result);
                                    result.AsyncWaitHandle.Close();

                                    result = device.BeginOpenContainer("Accelerated Delivery", null, null);
                                    result.AsyncWaitHandle.WaitOne();
                                    container = device.EndOpenContainer(result);
                                    result.AsyncWaitHandle.Close();
                                    goto FileNotFound;
                                case -1: // Fall through
                                case 2: returnOptionsToDefault();
                                    return;
                            }
                        } while(fileResult == null);
                    }
                }
            
                currentStream = container.OpenFile(filename, FileMode.Open);
                HandleEncryption(ref currentStream);
                file = (SaveFile)saveDataSerializer.Deserialize(currentStream);
                currentStream.Close();
                container.Dispose();
            }
            catch
            {
                try
                {
                    if(!initializing)
                    {
                        int? lalala;
                        do
                        {
                            lalala = SimpleMessageBox.ShowMessageBox("Warning", "The save file was missing or corrupted, but a backup file was found. Loading backup file.", new string[] { "Okay" }, 0, MessageBoxIcon.Alert);
                        } while(lalala == null);
                    }
                    backupStream = container.OpenFile(backupName, FileMode.Open);
                    HandleEncryption(ref backupStream);
                    file = (SaveFile)saveDataSerializer.Deserialize(backupStream);
                    currentStream = container.CreateFile(filename);
                    backupStream.Seek(0, SeekOrigin.Begin);
                    backupStream.CopyTo(currentStream);
                    currentStream.Close();
                    backupStream.Close();
                    container.Dispose();
                }
                catch(Exception ex)
                {
                    if(backupStream != null)
                        backupStream.Close();
                    if(currentStream != null)
                        currentStream.Close();
                    if(container != null)
                        container.Dispose();
                    throw ex;
                }
            }
#elif WINDOWS
            // Attempt to load primary save
            try
            {
                if(!File.Exists(filename))
                    if(!File.Exists(backupName))
                        New();

                currentStream = File.Open(filename, FileMode.Open);
                handleEncryption(ref currentStream);
                try
                {
                    file = (SaveFile<T>)saveDataSerializer.Deserialize(currentStream);
                }
                catch
                {
                    // maybe it got saved in plain-text accidentally so try again
                    currentStream.Seek(0, SeekOrigin.Begin); 
                    handleEncryption(ref currentStream);
                    file = (SaveFile<T>)saveDataSerializer.Deserialize(currentStream);
                }
                // at this point loading was successful
                // it will always be safe to copy to backup at this point
                currentStream.Seek(0, SeekOrigin.Begin);
                backupStream = File.Open(backupName, FileMode.Create);
                currentStream.CopyTo(backupStream);
                backupStream.Close();
                currentStream.Close();
                currentSave = file.currentSaveGame;
                SuccessfulLoad = true;
                Log.WriteToLog(this, "Load successful.");
            }
            catch(Exception ex)
            {
                Log.WriteToLog(this, "An error occurred while loading game data. Attempting to load backup save.");
                Log.WriteExceptionToLog(ex, false);
                try
                {
                    string dumpPath = Path.Combine(Path.GetDirectoryName(filename), "savdump.dmp");
                    FileStream dump = File.Open(dumpPath, FileMode.Create);
                    currentStream.Seek(0, SeekOrigin.Begin);
                    currentStream.CopyTo(dump);
                    dump.Close();
                    Log.WriteToLog(this, "Malformed save dumped to " + dumpPath + ".");
                }
                catch(Exception exc)
                {
                    Log.WriteToLog(this, "Could not dump save file.");
                    Log.WriteExceptionToLog(exc, false);
                }
                finally
                {
                    // Attempt to load backup save and copy data to current save
                    try
                    {
                        if(File.Exists(backupName))
                        {
                            backupStream = File.Open(backupName, FileMode.Open);
                            handleEncryption(ref backupStream);
                            file = (SaveFile<T>)saveDataSerializer.Deserialize(backupStream);

                            // if we're here, the backup was valid, so copy to primary
                            currentStream = File.Open(filename, FileMode.Create);
                            backupStream.Seek(0, SeekOrigin.Begin);
                            backupStream.CopyTo(currentStream);
                            currentSave = file.currentSaveGame;
                            SuccessfulLoad = true;
                            Log.WriteToLog(this, "Load from backup successful.");
                        }
                        else
                        {
                            Log.WriteToLog(this, "Backup save was not there. Load failed.");
                            askAboutFailedLoad();
                        }
                    }
                    catch
                    {
                        // Primary and backups are faulty, throw error
                        SuccessfulLoad = false;
                        Log.WriteToLog(this, "Primary and backup saves are faulty or missing. Load failed. Error is:");
                        Log.WriteExceptionToLog(ex, false);

                        askAboutFailedLoad();
                    }
                    finally
                    {
                        if(backupStream != null)
                            backupStream.Close();
                        if(currentStream != null)
                            currentStream.Close();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Asks the user what they want to do in the event of a failed load.
        /// </summary>
        protected virtual void askAboutFailedLoad()
        {
            DialogResult d = MessageBox.Show("The save file is corrupt. Is it okay to create a new save file? This will erase your progress.", "Error!", MessageBoxButtons.YesNo);
            if(d == DialogResult.Yes)
                New();
            else // no
            {
                // this is to alleviate a crash (hopefully); the game has to load before it
                // can exit
                doNotSave = true;
                Application.Exit(); // probably works faster than Game.Exit().
            }
        }

        /// <summary>
        /// Tries to load the file. Always seeks to the start of the file.
        /// </summary>
        /// <param name="s">Stream of a save file.</param>
        /// <returns>True if the load failed.</returns>
        protected bool tryLoad(Stream s)
        {
            try
            {
                handleEncryption(ref s);
                file = (SaveFile<T>)saveDataSerializer.Deserialize(s);
                handleEncryption(ref s);
            }
            catch
            {
                s.Seek(0, SeekOrigin.Begin);
                return true;
            }
            finally
            {
                s.Seek(0, SeekOrigin.Begin);
            }
            return false;
        }

        /// <summary>
        /// Encrypts or decrypts a stream. The default implementation does nothing.
        /// </summary>
        /// <remarks>The encryption must be symmetric; ie, the same algorithm must do both encryption
        /// and decryption.</remarks>
        /// <param name="stream">The Stream to encrypt.</param>
        protected void handleEncryption(ref Stream stream) { }

        /// <summary>
        /// Creates an empty savegame. Does not switch it to the current savegame.
        /// Warning: Will overwrite any save game in the current slot.
        /// </summary>
        /// <param name="saveGameIndex">1-based save index.</param>
        public void CreateNewSaveGame(int saveGameIndex)
        {
            try
            {
                file.Saves[saveGameIndex] = new T();
                file.Saves[saveGameIndex].BeenCreated = true;
            }
            catch(IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("saveGameIndex", "No such save file.");
            }
        }

        /// <summary>
        /// Switchs the current savegame. Will create a new savegame in that slot if one does not exist.
        /// </summary>
        /// <param name="saveGameIndex">1-based save index.</param>
        public void SwitchCurrentSave(int saveGameIndex)
        {
            currentSave = saveGameIndex - 1;
            if(!CurrentSave.BeenCreated)
                CreateNewSaveGame(currentSave + 1);
            SaveLoaded = true;

            OnSaveChanged();
        }

        /// <summary>
        /// Unloads the current save. 
        /// </summary>
        public void Unload()
        {
            SaveLoaded = false;
        }

        /// <summary>
        /// Gets one of the saves. Please don't modify values through this method.
        /// </summary>
        /// <param name="slot">1-based save index.</param>
        /// <returns>The save in that slot.</returns>
        public T GetSaveSlot(int slot)
        {
            try
            {
                return file.Saves[slot];
            }
            catch(IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("slot", "No such save file.");
            }
        }

        /// <summary>
        /// Deletes a file, permanently. Auto-saves afterward.
        /// </summary>
        /// <param name="i">1-based save index.</param>
        public void Delete(int i)
        {
            try
            {
                file.Saves[i] = new T();
            }
            catch(IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("slot", "No such save file.");
            }
            OnSaveDeleted(i);
            Save(true);
        }
    }
}