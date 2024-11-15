using System.Drawing;
using System.Text;

using PhotinoNET;

namespace DelugeManager.GUI;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Window title declared here for visibility
        string windowTitle = "Deluge Manager";

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle(windowTitle)
            // Resize to a percentage of the main monitor work area
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(1280, 720))
            // Center window in the middle of the screen
            .Center()
            .RegisterCustomSchemeHandler("app", SchemeHandler.HandleMessage)
            .SetJavascriptClipboardAccessEnabled(true)
            .SetSmoothScrollingEnabled(true)
            .SetContextMenuEnabled(false)
            .SetFileSystemAccessEnabled(true)
            // Most event handlers can be registered after the
            // PhotinoWindow was instantiated by calling a registration 
            // method like the following RegisterWebMessageReceivedHandler.
            // This could be added in the PhotinoWindowOptions if preferred.
            .RegisterWebMessageReceivedHandler((object sender, string message) => {
                var window = (PhotinoWindow)sender;

                // The message argument is coming in from sendMessage.
                // "window.external.sendMessage(message: string)"
                string response = $"Received message: \"{message}\"";

                // Send a message back the to JavaScript event handler.
                // "window.external.receiveMessage(callback: Function)"
                window.SendWebMessage(response);
            })
            .Load("wwwroot/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.

        window.WaitForClose(); // Starts the application event loop
    }
}
