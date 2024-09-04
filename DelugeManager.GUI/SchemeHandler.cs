using System.Text;

namespace DelugeManager.GUI;

public static class SchemeHandler
{
    public static Stream HandleMessage(object sender, string scheme, string url, out string contentType)
    {
        contentType = "text/javascript";
        return new MemoryStream(Encoding.UTF8.GetBytes($@"
            // async function a() {{
            //     alert(`🎉 Dynamically inserted JavaScript!`);
            // }}
            // a();
        "));
    }
}
