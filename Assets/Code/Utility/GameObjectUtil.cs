using UnityEngine;

namespace UnitySystemFramework.Utility
{
    public static class GameObjectUtil
    {
        /// <summary>
        /// Finds a component using a search path. Separate parts of the path using forward slashes. This will find
        /// the game object closest to the root with the the name matching first part of the path. After that it will
        /// try to match the rest of the path directly from that game object. EX: "SubmitBtn/Text" will get the Text
        /// component from the SubmitBtn in the game object's hierarchy.
        /// </summary>
        public static bool TryFindComponent<T>(this GameObject gameObject, string path, out T value) where T : Component
        {
            var found = gameObject.FindFromPath(path);
            if (found != null)
            {
                value = found.GetComponent<T>();
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Finds a component using a search path. Separate parts of the path using forward slashes. This will find
        /// the game object closest to the root with the the name matching first part of the path. After that it will
        /// try to match the rest of the path directly from that game object. EX: "SubmitBtn/Text" will get the Text
        /// component from the SubmitBtn in the game object's hierarchy.
        /// </summary>
        public static T FindComponent<T>(this GameObject gameObject, string path) where T : Component
        {
            var found = gameObject.FindFromPath(path);
            if (found != null)
                return found.GetComponent<T>();

            return default;
        }

        /// <summary>
        /// Finds a game object using a search path. Separate parts of the path using forward slashes. This will find
        /// the game object closest to the root with the the name matching first part of the path. After that it will
        /// try to match the rest of the path directly from that game object. EX: "SubmitBtn/Text" will get the Text
        /// GameObject from the SubmitBtn in the game object's hierarchy.
        /// </summary>
        public static GameObject FindFromPath(this GameObject gameObject, string path)
        {
            if (path == "/")
                return gameObject.transform.root?.gameObject ?? gameObject;

            // TODO: Remove the string split.
            return FindFromPath(path.Split('/'), 0, gameObject.transform)?.gameObject;
        }

        // TODO: Don't use a string array, it encourages using string split.
        private static Transform FindFromPath(string[] path, int index, Transform parent)
        {
            if (parent.name == path[index] || parent.name == path[index] + "(Clone)")
            {
                index++;
                if (index == path.Length)
                    return parent;
            }

            // TODO: Change this to look at all direct children before digging further in the hierarchy.
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                var found = FindFromPath(path, index, child);
                if (found)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Gets the full path from the root transform to this transform.
        /// </summary>
        public static string GetPath(this Transform transform)
        {
            if (transform.parent == null)
                return "/" + transform.name;
            return string.Join("/", GetPath(transform.parent), transform.name);
        }

        /// <summary>
        /// Gets the full path from the root transform to this transform.
        /// </summary>
        public static string GetPath(this GameObject gameObject)
        {
            if (gameObject.transform.parent == null)
                return "/" + gameObject.name;
            return string.Join("/", GetPath(gameObject.transform.parent), gameObject.name);
        }
    }
}
