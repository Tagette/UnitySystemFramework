using UnitySystemFramework.Accounts;
using UnitySystemFramework.Core;
using UnitySystemFramework.Identity;

namespace UnitySystemFramework.Assets.Code.Accounts
{
    public class DefaultAccountSystem : BaseSystem, IAccountSystem
    {
        public bool IsOnline => false;

        public bool IsLoggedIn => false;

        public ID<Account> AccountID => default;

        public string AccountName => null;

        public string DisplayName => null;

        protected override void OnInit()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
        }
    }
}
