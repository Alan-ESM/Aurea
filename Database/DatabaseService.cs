using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Aurea.Database
{
    public class DatabaseService
    {
        private string connectionString;
        private string cacheDirectory;

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Créer le dossier cache
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Aurea");
            cacheDirectory = Path.Combine(appDataPath, "Cache");

            Directory.CreateDirectory(cacheDirectory);

            // Créer la base de données
            string dbPath = Path.Combine(appDataPath, "aurea.db");
            connectionString = $"Data Source={dbPath};Version=3;";

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            // Garantir le schéma même si le fichier de base existe déjà.
            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createDocumentsTable = @"
                    CREATE TABLE IF NOT EXISTS Documents (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FilePath TEXT NOT NULL,
                        FileName TEXT NOT NULL,
                        Content TEXT,
                        ContentHash TEXT,
                        LastModified DATETIME,
                        CreatedDate DATETIME,
                        FileSize INTEGER,
                        CachePath TEXT
                    )";

                string createCollaboratorsTable = @"
                    CREATE TABLE IF NOT EXISTS Collaborators (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DocumentId INTEGER,
                        Name TEXT,
                        Email TEXT,
                        Role TEXT,
                        AddedDate DATETIME,
                        FOREIGN KEY(DocumentId) REFERENCES Documents(Id)
                    )";

                string createHistoryTable = @"
                    CREATE TABLE IF NOT EXISTS History (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DocumentId INTEGER,
                        Action TEXT,
                        Timestamp DATETIME,
                        UserName TEXT,
                        Details TEXT,
                        FOREIGN KEY(DocumentId) REFERENCES Documents(Id)
                    )";

                string createBackupsTable = @"
                    CREATE TABLE IF NOT EXISTS Backups (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DocumentId INTEGER,
                        BackupPath TEXT,
                        BackupDate DATETIME,
                        IsAutoBackup BOOLEAN,
                        FOREIGN KEY(DocumentId) REFERENCES Documents(Id)
                    )";

                using (var command = new SQLiteCommand(createDocumentsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(createCollaboratorsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(createHistoryTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(createBackupsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveDocumentBackup(string filePath, string content)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string fileName = Path.GetFileName(filePath);
                    string contentHash = ComputeHash(content);
                    DateTime now = DateTime.Now;

                    // Vérifier si le document existe déjà
                    string checkQuery = "SELECT Id, ContentHash FROM Documents WHERE FilePath = @FilePath";
                    int? documentId = null;
                    string existingHash = null;

                    using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@FilePath", filePath);
                        using (var reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                documentId = reader.GetInt32(0);
                                existingHash = reader.IsDBNull(1) ? null : reader.GetString(1);
                            }
                        }
                    }

                    // Ne sauvegarder que si le contenu a changé
                    if (existingHash != contentHash)
                    {
                        // Créer une copie dans le cache
                        string cachePath = Path.Combine(
                            cacheDirectory,
                            $"{Path.GetFileNameWithoutExtension(fileName)}_{now:yyyyMMddHHmmssfff}_{contentHash[..8]}{Path.GetExtension(fileName)}"
                        );

                        if (File.Exists(cachePath))
                        {
                            File.SetAttributes(cachePath, FileAttributes.Normal);
                        }
                        File.Copy(filePath, cachePath, true);

                        // Protéger le fichier (lecture seule)
                        File.SetAttributes(cachePath, FileAttributes.ReadOnly);

                        if (documentId.HasValue)
                        {
                            // Mettre à jour
                            string updateQuery = @"
                                UPDATE Documents 
                                SET Content = @Content, 
                                    ContentHash = @ContentHash, 
                                    LastModified = @LastModified,
                                    CachePath = @CachePath,
                                    FileSize = @FileSize
                                WHERE Id = @Id";

                            using (var cmd = new SQLiteCommand(updateQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@Content", content);
                                cmd.Parameters.AddWithValue("@ContentHash", contentHash);
                                cmd.Parameters.AddWithValue("@LastModified", now);
                                cmd.Parameters.AddWithValue("@CachePath", cachePath);
                                cmd.Parameters.AddWithValue("@FileSize", content.Length);
                                cmd.Parameters.AddWithValue("@Id", documentId.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insérer
                            string insertQuery = @"
                                INSERT INTO Documents 
                                (FilePath, FileName, Content, ContentHash, LastModified, CreatedDate, FileSize, CachePath)
                                VALUES 
                                (@FilePath, @FileName, @Content, @ContentHash, @LastModified, @CreatedDate, @FileSize, @CachePath)";

                            using (var cmd = new SQLiteCommand(insertQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@FilePath", filePath);
                                cmd.Parameters.AddWithValue("@FileName", fileName);
                                cmd.Parameters.AddWithValue("@Content", content);
                                cmd.Parameters.AddWithValue("@ContentHash", contentHash);
                                cmd.Parameters.AddWithValue("@LastModified", now);
                                cmd.Parameters.AddWithValue("@CreatedDate", now);
                                cmd.Parameters.AddWithValue("@FileSize", content.Length);
                                cmd.Parameters.AddWithValue("@CachePath", cachePath);
                                cmd.ExecuteNonQuery();
                            }

                            // Récupérer l'ID du nouveau document
                            documentId = (int)connection.LastInsertRowId;
                        }

                        // Enregistrer dans les backups
                        string backupQuery = @"
                            INSERT INTO Backups (DocumentId, BackupPath, BackupDate, IsAutoBackup)
                            VALUES (@DocumentId, @BackupPath, @BackupDate, @IsAutoBackup)";

                        using (var cmd = new SQLiteCommand(backupQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@DocumentId", documentId.Value);
                            cmd.Parameters.AddWithValue("@BackupPath", cachePath);
                            cmd.Parameters.AddWithValue("@BackupDate", now);
                            cmd.Parameters.AddWithValue("@IsAutoBackup", true);
                            cmd.ExecuteNonQuery();
                        }

                        // Ajouter à l'historique
                        LogHistory(documentId.Value, "SAVE", Environment.UserName,
                                 $"Document sauvegardé : {fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la sauvegarde dans la BD : {ex.Message}");
            }
        }

        public string RecoverDocument(string filePath)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT CachePath FROM Documents WHERE FilePath = @FilePath ORDER BY LastModified DESC LIMIT 1";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FilePath", filePath);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            string cachePath = result.ToString();
                            if (File.Exists(cachePath))
                            {
                                // Créer une copie temporaire pour la récupération
                                string recoveredPath = Path.Combine(
                                    Path.GetDirectoryName(filePath),
                                    $"RECOVERED_{Path.GetFileName(filePath)}"
                                );

                                File.Copy(cachePath, recoveredPath, true);
                                File.SetAttributes(recoveredPath, FileAttributes.Normal);
                                return recoveredPath;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la récupération : {ex.Message}");
            }

            return null;
        }

        public void AddCollaborator(int documentId, string name, string email, string role)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO Collaborators (DocumentId, Name, Email, Role, AddedDate)
                    VALUES (@DocumentId, @Name, @Email, @Role, @AddedDate)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LogHistory(int documentId, string action, string userName, string details)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO History (DocumentId, Action, Timestamp, UserName, Details)
                    VALUES (@DocumentId, @Action, @Timestamp, @UserName, @Details)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@Details", details);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string ComputeHash(string content)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public void CleanOldBackups(int daysToKeep = 30)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Récupérer les anciens backups
                string selectQuery = "SELECT BackupPath FROM Backups WHERE BackupDate < @CutoffDate";

                using (var cmd = new SQLiteCommand(selectQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@CutoffDate", cutoffDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string backupPath = reader.GetString(0);
                            if (File.Exists(backupPath))
                            {
                                File.SetAttributes(backupPath, FileAttributes.Normal);
                                File.Delete(backupPath);
                            }
                        }
                    }
                }

                // Supprimer les entrées de la BD
                string deleteQuery = "DELETE FROM Backups WHERE BackupDate < @CutoffDate";
                using (var cmd = new SQLiteCommand(deleteQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@CutoffDate", cutoffDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
