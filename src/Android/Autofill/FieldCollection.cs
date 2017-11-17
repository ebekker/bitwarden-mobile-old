﻿using System.Collections.Generic;
using Android.Service.Autofill;
using Android.Views.Autofill;
using System.Linq;
using Android.Text;

namespace Bit.Android.Autofill
{
    public class FieldCollection
    {
        public HashSet<int> Ids { get; private set; } = new HashSet<int>();
        public List<AutofillId> AutofillIds { get; private set; } = new List<AutofillId>();
        public SaveDataType SaveType { get; private set; } = SaveDataType.Generic;
        public List<string> Hints { get; private set; } = new List<string>();
        public List<string> FocusedHints { get; private set; } = new List<string>();
        public List<Field> Fields { get; private set; } = new List<Field>();
        public IDictionary<int, Field> IdToFieldMap { get; private set; } =
            new Dictionary<int, Field>();
        public IDictionary<string, List<Field>> HintToFieldsMap { get; private set; } =
            new Dictionary<string, List<Field>>();

        public void Add(Field field)
        {
            if(Ids.Contains(field.Id))
            {
                return;
            }

            Ids.Add(field.Id);
            Fields.Add(field);
            SaveType |= field.SaveType;
            AutofillIds.Add(field.AutofillId);
            IdToFieldMap.Add(field.Id, field);

            if((field.Hints?.Count ?? 0) > 0)
            {
                Hints.AddRange(field.Hints);
                if(field.Focused)
                {
                    FocusedHints.AddRange(field.Hints);
                }

                foreach(var hint in field.Hints)
                {
                    if(!HintToFieldsMap.ContainsKey(hint))
                    {
                        HintToFieldsMap.Add(hint, new List<Field>());
                    }

                    HintToFieldsMap[hint].Add(field);
                }
            }
        }

        public SavedItem GetSavedItem()
        {
            if(!Fields?.Any() ?? true)
            {
                return null;
            }

            var passwordField = Fields.FirstOrDefault(
                f => f.InputType.HasFlag(InputTypes.TextVariationPassword) && !string.IsNullOrWhiteSpace(f.TextValue));
            if(passwordField == null)
            {
                passwordField = Fields.FirstOrDefault(
                    f => (f.IdEntry?.ToLower().Contains("password") ?? false) && !string.IsNullOrWhiteSpace(f.TextValue));
            }

            if(passwordField == null)
            {
                return null;
            }

            var savedItem = new SavedItem
            {
                Type = App.Enums.CipherType.Login,
                Login = new SavedItem.LoginItem
                {
                    Password = passwordField.TextValue
                }
            };

            var usernameField = Fields.TakeWhile(f => f.Id != passwordField.Id).LastOrDefault();
            if(usernameField != null && !string.IsNullOrWhiteSpace(usernameField.TextValue))
            {
                savedItem.Login.Username = usernameField.TextValue;
            }

            return savedItem;
        }
    }
}