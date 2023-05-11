using System;

using Newtonsoft.Json;

namespace General
{
    public class Share : IEquatable<Share>
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Type { get; set; }

        [JsonIgnore] public string Path => System.IO.Path.Combine(Folder, Name);

        public bool Equals(Share other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name && Folder == other.Folder && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;

            return Equals((Share) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Folder != null ? Folder.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}