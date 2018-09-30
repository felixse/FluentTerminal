using System;
using System.Collections.Generic;
using System.Text;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    /// <summary>
    /// Wrapper class to abstract handling both static enum based commands, as well as dynamic commands such as per-shell shortcuts.
    /// </summary>
    public abstract class AbstractCommand
    {
        public abstract string Description { get; }
        public abstract override int GetHashCode();
        public abstract override bool Equals(object obj);
        public abstract override string ToString();

        public static bool operator ==(AbstractCommand A, AbstractCommand B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(AbstractCommand A, AbstractCommand B)
        {
            return !A.Equals(B);
        }

        public static implicit operator AbstractCommand(Command input)
        {
            return new EnumCommand<Command>(input);
        }

        public static implicit operator AbstractCommand(ShellProfile profile)
        {
            return new NewShellTerminal(profile);
        }
    }

    public class EnumCommand<EnumType> : AbstractCommand
    {
        EnumType val;

        public EnumCommand(EnumType enumValue)
        {
            val = enumValue;
        }

        public override string Description => Enum.GetName(typeof(EnumType), val);

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is EnumCommand<EnumType>)
            {
                return GetHashCode() == obj.GetHashCode();
            }
            else if (obj is EnumType)
            {
                EnumType other = (EnumType)obj;
                return val.GetHashCode() == other.GetHashCode();
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            // TODO This should use the FluentTerminal.App.Services.Utilities.EnumHelper helper, but that will need to be moved out into a new project to handle dependencies.
            return val.ToString();
        }
    }

    public class NewShellTerminal : AbstractCommand
    {
        ShellProfile profile;

        public NewShellTerminal(ShellProfile profile)
        {
            this.profile = profile;
        }

        public override string Description => profile.Name;

        public override int GetHashCode()
        {
            // Every shell's GUID is unique within this scope, so just return that hash code.
            return profile.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is NewShellTerminal)
            {
                ShellProfile other = obj as ShellProfile;
                return GetHashCode() == other.GetHashCode();
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return profile.Id.ToString();
        }
    }
}
