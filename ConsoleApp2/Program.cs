namespace ConsoleApp2
{
    class Program
    {
        private static bool isDeviceConnected = false;

        static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task monitorTask = MonitorDeviceConnection(cts.Token);

            while (true)
            {
                try
                {
                    await WaitForDeviceConnection();

                    // Ask for count
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Enter tap count:");
                    Console.ResetColor();

                    if (int.TryParse(Console.ReadLine(), out int count))
                    {
                        var tapCts = new CancellationTokenSource();
                        var tapToken = tapCts.Token;

                        // Start a task to listen for the Enter key press
                        Task.Run(() =>
                        {
                            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            {
                                tapCts.Cancel();
                            }
                        });

                        await ExecuteTapCommand(count, tapToken);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Invalid input for count. Please enter a valid number.");
                        Console.ResetColor();
                        await Task.Delay(2000); // Keep the invalid input message visible for 2 seconds
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }

        static async Task WaitForDeviceConnection()
        {
            while (!ADBHelper.IsDeviceConnected())
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\rNo device connected. Please connect a device.      ");
                Console.ResetColor();

                // Wait for a short period before checking again
                await Task.Delay(1000);
            }

            string deviceModel = ADBHelper.GetDeviceModel();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Device connected: {deviceModel}");
            Console.ResetColor();
        }

        static async Task ExecuteTapCommand(int count, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nExecution canceled");
                    Console.ResetColor();
                    break;
                }

                var random = new Random();
                var size = ADBHelper.GetDisplaySize();
                var minWidth = (size.Width / 2) - 200;
                var maxWidth = (size.Width / 2) + 200;
                var minHeight = size.Height / 2;
                var maxHeight = (size.Height / 2) + 200;

                ADBHelper.RunADBCommand($"shell input tap {random.Next(minWidth, maxWidth)} {random.Next(minHeight, maxHeight)}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"\rStart tapping: {i + 1}");
                Console.ResetColor();
                await Task.Delay(50);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nExecution completed.");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        static async Task OpenHamster(int count, CancellationToken cancellationToken = default)
        {
            if (ADBHelper.IsPhoneLocked())
            {
                // Unlock the phone
                UnlockPhone();
                // Wait for the phone to unlock
                await Task.Delay(2000);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
            string botLink = "tg://resolve?domain=hamster_kombat_bot";
            ADBHelper.RunADBCommand($"shell am start -a android.intent.action.VIEW -d \"{botLink}\"");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Open hamster");
            Console.ResetColor();
            ADBHelper.TapOnScreen(50, 2350);
            await Task.Delay(TimeSpan.FromSeconds(1));
            ADBHelper.TapOnScreen(50, 2350);
            await Task.Delay(TimeSpan.FromSeconds(10));
            ADBHelper.TapOnScreen(500, 2200);
            await ExecuteTapCommand(count, cancellationToken);
        }

        static void UnlockPhone()
        {
            ADBHelper.RunADBCommand("shell input keyevent 26"); // Power Button Pressed.
            ADBHelper.RunADBCommand("shell input keyevent 82"); // Swipe Up.
            ADBHelper.RunADBCommand("shell input swipe 300 1000 300 500");
            ADBHelper.RunADBCommand("shell input text 2566"); // Enter your password/pin here.
        }

        static async Task MonitorDeviceConnection(CancellationToken token)
        {
            bool previousConnectionStatus = ADBHelper.IsDeviceConnected();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    bool currentConnectionStatus = ADBHelper.IsDeviceConnected();

                    if (currentConnectionStatus != previousConnectionStatus)
                    {
                        previousConnectionStatus = currentConnectionStatus;
                        Console.Clear();
                        if (currentConnectionStatus)
                        {
                            string deviceModel = ADBHelper.GetDeviceModel();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\nDevice connected: {deviceModel}");
                            Console.ResetColor();
                            isDeviceConnected = true;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("\rNo device connected. Please connect a device.");
                            Console.ResetColor();
                            isDeviceConnected = false;
                        }
                    }

                    await Task.Delay(3000, token);
                }
                catch (TaskCanceledException)
                {
                    // Handle the task cancellation exception only if necessary
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }
    }
}
