using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.Models
{
    /// <summary>
    /// Wrapper class to abstract handling both static enum based commands, as well as dynamic commands such as per-shell shortcuts.
    /// </summary>
    public interface ICommand
    {
        string Description();
        int GetHashCode();
        bool Equals(object obj);
    }

    public class EnumCommand<EnumType> : ICommand
    {
        EnumType val;

        public EnumCommand(EnumType enumValue)
        {
            val = enumValue;
        }

        public string Description()
        {
            return Enum.GetName(typeof(EnumType), val);
        }

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
    }

    public interface IDynamicCommand : ICommand
    {

    }

    public class NewShellTerminal : IDynamicCommand
    {
        public NewShellTerminal()
        {

        }

        public string Description()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is NewShellTerminal)
            {
                return Equals(obj as NewShellTerminal);
            }
            else
            {
                return false;
            }
        }
    }
}
