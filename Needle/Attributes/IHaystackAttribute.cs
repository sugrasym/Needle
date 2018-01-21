namespace IMS.Common.Needle.Attributes
{
    public interface IHaystackAttribute
    {
        string Path { get; set; }
        string IsVisible { get; set; }
        string QueryRender { get; set; }
        string DisplayName { get; set; }
    }
}