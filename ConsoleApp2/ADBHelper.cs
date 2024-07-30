using System.Diagnostics;

namespace ConsoleApp2;

public class ADBHelper
{
    public static string RunADBCommand(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            WorkingDirectory = @"C:\Users\yaghm\Downloads\Compressed\platform-tools-latest-windows\platform-tools",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = "c:\\Windows\\system32\\cmd.exe",
            Arguments = "/c adb " + command,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process() { StartInfo = startInfo })
        {
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
    public static bool IsDeviceConnected()
    {
        string output = RunADBCommand("devices");

        // The output will have a list of connected devices if any are present
        // Typically, the output will include lines with the format:
        // List of devices attached
        // emulator-5554   device
        // If no devices are connected, it may just have "List of devices attached" with no subsequent lines.
        string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        foreach (string line in lines)
        {
            if (line.Contains("\tdevice"))
            {
                return true;
            }
        }
        return false;
    }
    public static string GetDeviceModel()
    {
        string output = RunADBCommand("shell getprop ro.product.model");
        return output.Trim();
    }

    public static bool IsPhoneLocked()
    {
        string output = RunADBCommand("shell dumpsys window");

        // Check for indications that the phone is locked
        bool isLocked = output.Contains("mDreamingLockscreen=true") || output.Contains("mUserActivityTimeoutOverrideFromWindowManager=-1");

        return isLocked;
    }
    public static void TapOnScreen(int x, int y)
    {
        RunADBCommand($"shell input tap {x} {y}");
        Console.WriteLine($"Tap on screen on {x} {y}");

    }
    public static void OpenApplication(string packageName)
    {
        string output = RunADBCommand("shell dumpsys window windows | grep mCurrentFocus");

        // Check if the output contains the package name
        if (!output.Contains(packageName))
        {
            RunADBCommand($"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1");
        }

        Console.WriteLine($"Open package : {packageName}");
    }
    public static (int Width, int Height) GetDisplaySize()
    {
        string output = RunADBCommand("shell wm size");
        // Output will be something like: "Physical size: 1080x1920"
        string sizePrefix = "Physical size: ";
        int prefixIndex = output.IndexOf(sizePrefix);
        if (prefixIndex != -1)
        {
            string sizeString = output.Substring(prefixIndex + sizePrefix.Length).Trim();
            string[] dimensions = sizeString.Split('x');
            if (dimensions.Length == 2 &&
                int.TryParse(dimensions[0], out int width) &&
                int.TryParse(dimensions[1], out int height))
            {
                return (width, height);
            }
        }
        throw new InvalidOperationException("Unable to determine display size.");
    }

    public static void UnlockPhone(string? pass = null)
    {
        ADBHelper.RunADBCommand("shell input keyevent 26"); // Power Button Pressed.
        ADBHelper.RunADBCommand("shell input keyevent 82");
        ADBHelper.RunADBCommand("shell input swipe 300 1000 300 500"); // Swipe Up.
        if (!string.IsNullOrWhiteSpace(pass))
            ADBHelper.RunADBCommand($"shell input text {pass}"); // Enter your password/pin here.
    }

}
