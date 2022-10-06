using UnityEngine;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitySystemFramework.Utility {
    public static class FileUtil {
        private static int sleepAttempt = 5;
        private static int maxAttempts = 3;

        private static void HandleCatch(string msg) {
            Debug.LogWarning(msg);
            Thread.Sleep(FileUtil.sleepAttempt);
        }

        public static FileStream Open(string path, int attempt = 1) {
            FileStream fileStream = null;
            try {
                fileStream = File.Open(path, FileMode.Open);
            } catch (System.Exception) {
                HandleCatch("Failed to open file: " + path + " " + " attempts: " + attempt);
                attempt++;
                if (attempt > maxAttempts) {
                    return fileStream;
                }
                return Open(path, attempt);
            }

            return fileStream;
        }

        public static FileStream Move(string path, string to, int attempt = 1) {
            try {
                File.Move(path, to);
            } catch (System.Exception) {
                HandleCatch("Failed to move file: " + path + " to: " + to + " attempts: " + attempt);
                attempt++;
                if (attempt > maxAttempts) {
                    return null;
                }
                return Move(path, to, attempt);
            }

            return null;
        }

        public static FileStream Create(string path, object content, int attempt = 1) {
            FileStream fileStream = null;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            try {
                fileStream = File.Create(path);
                binaryFormatter.Serialize(fileStream, content);
                fileStream.Close();
            } catch (System.Exception) {
                HandleCatch("Failed to create file: " + path + " attempts: " + attempt);
                attempt++;
                if (attempt > maxAttempts) {
                    return null;
                }
                return Create(path, content, attempt);
            }


            return fileStream;
        }
    }
}

