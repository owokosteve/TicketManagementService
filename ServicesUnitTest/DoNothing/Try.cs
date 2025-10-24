using System;

namespace ServicesUnitTest.DoNothing
{
    /// <summary>
    /// Default Try class specializing in Status3.
    /// </summary>
    public class Try : Try<Status3>
    {
        public Try(string startString) : base(startString) { }
    }

    /// <summary>
    /// A generic class capable of performing actions on an enum type.
    /// </summary>
    /// <typeparam name="T">Enum type the class will operate on.</typeparam>
    public class Try<T> where T : Enum
    {
        private T _predicate;
        private readonly string startString = "Start";

        public Try(string startString)
        {
            if (string.IsNullOrWhiteSpace(startString))
            {
                throw new ArgumentException("Start string cannot be null or whitespace.", nameof(startString));
            }
            this.startString = startString;
        }

        /// <summary>
        /// Performs an action that essentially 'does nothing' but demonstrates functionality.
        /// </summary>
        /// <param name="val">The enumeration value to process and display.</param>
        public void DoNothing(T val = default)
        {
            Console.WriteLine($"{this.startString}: {val}");
        }
    }

    /// <summary>
    /// A class to demonstrate usage of the Try classes.
    /// </summary>
    public class Caller
    {
        public void Call()
        {
            var tryInstance = new Try<Status1>("Base");
            tryInstance.DoNothing(Status1.Success);

            var tryInstance2 = new Try<Status2>("Upper");
            tryInstance2.DoNothing(Status2.Passed);

            var tryInstance3 = new Try("Lower");
            tryInstance3.DoNothing(Status3.Accepted);
            tryInstance3.DoNothing();
        }
    }

    public enum Status1
    {
        Success,
        Lost
    }

    public enum Status2
    {
        Passed,
        Failed
    }

    public enum Status3
    {
        Accepted,
        Rejected
    }
}