using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace manager.aiv.it
{
    public enum SubmitType
    {
        [SubmitHandler(typeof(BriefSummaryHandler))]
        BriefSummary = 0,

        [SubmitHandler(typeof(ImageHandler))]
        Image = 1,

        [SubmitHandler(typeof(ZipHandler))]
        Zip,
    }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubmitHandlerAttribute : Attribute
    {
        public Type Type { get; private set; }
        public SubmitHandlerAttribute(Type hType)
        {
            Type = hType;
        }

        public static ISomeInterface GetImplementation(SubmitType eType)
        {
            FieldInfo hField = eType.GetType().GetField(eType.ToString());

            SubmitHandlerAttribute hAllocator = hField.GetCustomAttributes<SubmitHandlerAttribute>().First();

            return hAllocator.GetImplementation();
        }

        private ISomeInterface GetImplementation()
        {
            return Activator.CreateInstance(this.Type) as ISomeInterface;
        }

    }


    public interface ISomeInterface
    {
    }

    public class BriefSummaryHandler : ISomeInterface
    {

    }

    public class ImageHandler : ISomeInterface
    {

    }

    public class ZipHandler : ISomeInterface
    {

    }
}