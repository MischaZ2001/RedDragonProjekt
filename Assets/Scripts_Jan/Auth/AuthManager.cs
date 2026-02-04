using System;
using System.Linq;
using UnityEngine;

namespace RedDragon
{
    public enum AuthMode
    {
        Free,
        LoggedIn
    }

    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        public AuthMode Mode { get; private set; } = AuthMode.Free;
        public string CurrentUser { get; private set; }

        public event Action<AuthMode, string> OnAuthStateChanged;

        private AccountsDb db;

        private void Awake()
        {
            Debug.Log("[AuthManager] Awake");
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            db = AccountStore.Load();

            // Startzustand ist Free – aber UI entscheidet, dass zuerst Login gezeigt wird (AuthBoot).
            SetFree(silent: true);
        }

        public bool TryLogin(string username, string password, out string error)
        {
            error = null;

            username = (username ?? "").Trim();
            password ??= "";

            if (username.Length == 0 || password.Length == 0)
            {
                error = "Bitte Username und Passwort eingeben.";
                return false;
            }

            var acc = db.accounts.FirstOrDefault(a =>
                a.username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (acc == null)
            {
                error = "Account nicht gefunden.";
                return false;
            }

            var hash = PasswordHasher.Hash(password, acc.salt);
            if (hash != acc.passwordHash)
            {
                error = "Passwort falsch.";
                return false;
            }

            SetLoggedIn(acc.username);
            return true;
        }

        public bool TrySignUp(string username, string password, out string error)
        {
            error = null;

            username = (username ?? "").Trim();
            password ??= "";

            if (username.Length == 0 || password.Length == 0)
            {
                error = "Bitte Username und Passwort eingeben.";
                return false;
            }

            if (username.Length < 3)
            {
                error = "Username muss mindestens 3 Zeichen haben.";
                return false;
            }

            if (password.Length < 4)
            {
                error = "Passwort muss mindestens 4 Zeichen haben.";
                return false;
            }

            if (db.accounts.Any(a => a.username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                error = "Dieser Username ist bereits vergeben.";
                return false;
            }

            var salt = PasswordHasher.CreateSalt();
            var hash = PasswordHasher.Hash(password, salt);

            db.accounts.Add(new AccountRecord
            {
                username = username,
                salt = salt,
                passwordHash = hash,
                isPremium = true
            });

            AccountStore.Save(db);

            SetLoggedIn(username);
            return true;
        }

        public void SetFree(bool silent = false)
        {
            Mode = AuthMode.Free;
            CurrentUser = null;

            if (!silent)
                OnAuthStateChanged?.Invoke(Mode, CurrentUser);
        }

        public void SetLoggedIn(string username)
        {
            Mode = AuthMode.LoggedIn;
            CurrentUser = username;

            OnAuthStateChanged?.Invoke(Mode, CurrentUser);
        }

        public void LogoutToFree()
        {
            SetFree();
        }
    }
}
