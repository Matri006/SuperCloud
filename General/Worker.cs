using System;

namespace General
{
    public class Worker : IEquatable<Worker>
    {
        public string Type { get; set; }
        public string Name { get; set; } 
        public string Password { get; set; }

        public bool Equals(Worker other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Type == other.Type && Name == other.Name && Password == other.Password;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals((Worker) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}