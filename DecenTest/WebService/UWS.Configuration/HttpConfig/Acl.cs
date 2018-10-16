namespace HttpConfig
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class Acl : ICloneable
    {
        private List<Ace> _acl;

        internal Acl()
        {
            this._acl = new List<Ace>();
        }

        internal Acl(Ace initialAce)
        {
            this._acl = new List<Ace>();
            this._acl.Add(initialAce);
        }

        internal Acl(string user) : this(new Ace(user))
        {
        }

        internal static Acl FromSddl(string sddl)
        {
            Acl acl = new Acl();
            string[] strArray = sddl.Split(new char[] { '(', ')' });
            for (int i = 1; i < strArray.Length; i++)
            {
                if ((i % 2) > 0)
                {
                    acl._acl.Add(Ace.FromSddl(strArray[i]));
                }
            }
            return acl;
        }

        internal bool MatchesUser(string userName)
        {
            if (this._acl.Count != 1)
            {
                return false;
            }
            return (this._acl[0].User.ToLowerInvariant() == userName.ToLowerInvariant());
        }

        internal void SetUser(string user)
        {
            this._acl.Clear();
            this.Aces.Add(new Ace(user));
        }

        object ICloneable.Clone()
        {
            Acl acl = new Acl();
            foreach (Ace ace in this._acl)
            {
                acl._acl.Add(new Ace(ace.User, ace.AccountNameMapped, ace.Permission, ace.OtherPerm));
            }
            return acl;
        }

        internal string ToSddl()
        {
            StringBuilder sddl = new StringBuilder();
            sddl.Append("D:");
            foreach (Ace ace in this._acl)
            {
                ace.AddSddl(sddl);
            }
            return sddl.ToString();
        }

        internal List<Ace> Aces
        {
            get
            {
                return this._acl;
            }
        }

        internal string FriendlyString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (Ace ace in this._acl)
                {
                    builder.Append("(");
                    builder.Append(ace.User);
                    builder.Append(";");
                    builder.Append(ace.Permission.ToString());
                    builder.Append(")");
                }
                return builder.ToString();
            }
        }
    }
}

