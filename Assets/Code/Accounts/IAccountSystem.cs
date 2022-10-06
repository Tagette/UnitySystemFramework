using UnitySystemFramework.Core;
using UnitySystemFramework.Identity;

namespace UnitySystemFramework.Accounts
{
    public interface IAccountSystem : ISystem
    {
        /// <summary>
        /// Whether or not the account system is currently connected to the online services.
        /// </summary>
        bool IsOnline { get; }

        /// <summary>
        /// Whether or not the current user is logged in.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// The ID for the account.
        /// </summary>
        ID<Account> AccountID { get; }

        /// <summary>
        /// Gets the user name of the account. Not to be displayed to the user.
        /// </summary>
        string AccountName { get; }

        /// <summary>
        /// The display name of the account. Can be displayed to users.
        /// </summary>
        string DisplayName { get; }
    }
}
