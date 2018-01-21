using System;

namespace IMS.Common.Needle.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class HaystackAttribute : Attribute, IHaystackAttribute
    {
        public HaystackAttribute(string path = null,
            string isVisible = null,
            string queryRender = null)
        {
            Path = path;
            IsVisible = isVisible;
            QueryRender = queryRender;
        }

        public string Path { get; set; }
        public string IsVisible { get; set; }
        public string QueryRender { get; set; }
        public string DisplayName { get; set; }
    }
}