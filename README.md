# InstanceManager
.net instance handling

# Example usage

```
class Program
{
   static void Main(string[] args)
   {
      try
      {
         InstanceManager.InstanceManager instanceManager = new InstanceManager.InstanceManager(args);

         Console.WriteLine("I am the first instance.");
         // Run startUp like a new instance would. This is optional and could go down a seperate path.
         startUp(args);
         instanceManager.InstanceStartup(startUp);
      }
      catch (Exception e)
      {
         if (e.Message.Equals("Already running"))
         {
            return;
         }
         throw e;
      }
      Task.Delay(-1).Wait();         
   }

   private static void startUp(string[] args)
   {
      Console.WriteLine("New args: ");
      foreach (string arg in args) {
         Console.Write(arg + ", ");
      }
      Console.WriteLine("");
   }
}
```
