namespace Memory_Management;

class Program
{
    static void Main()
    {
        try
        {
            // Basic example usage 

            IntPtr memoryAddress = Memory.AllocateMemory(sizeof(int));

            int valueToWrite = 42;
            Memory.WriteToMemory(memoryAddress, valueToWrite);

            int valueRead = Memory.ReadFromMemory<int>(memoryAddress);

            Console.WriteLine($"Value written to memory: {valueToWrite}");
            Console.WriteLine($"Value read from memory: {valueRead}");

            Memory.FreeMemory(memoryAddress);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
