using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RedDragon
{
    [Serializable]
    public class AccountRecord
    {
        public string username;
        public string salt;
        public string passwordHash;
        public bool isPremium = true; // Account => Premium
    }

    [Serializable]
    public class AccountsDb
    {
        public List<AccountRecord> accounts = new();
    }

    public static class AccountStore
    {
        private const string FileName = "accounts.json";

        public static string GetPath()
            => Path.Combine(Application.persistentDataPath, FileName);

        public static AccountsDb Load()
        {
            var path = GetPath();

            if (!File.Exists(path))
                return new AccountsDb();

            try
            {
                var json = File.ReadAllText(path);
                var db = JsonUtility.FromJson<AccountsDb>(json);
                return db ?? new AccountsDb();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AccountStore] Load failed: {e}");
                return new AccountsDb();
            }
        }

        public static void Save(AccountsDb db)
        {
            try
            {
                var json = JsonUtility.ToJson(db, true);
                File.WriteAllText(GetPath(), json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AccountStore] Save failed: {e}");
            }
        }
    }
}
