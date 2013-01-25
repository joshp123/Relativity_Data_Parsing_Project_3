using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
// used for parsing the file

namespace Relativity_Data_Parsing_Project_3
{
    class Program
    {
        /// <summary>
        /// Event structure
        /// </summary>
        /// <remarks>
        /// Contains fields for times and energy
        /// </remarks>
        public struct Event
        {
            /// <summary>
            /// Time when particle detected at first detector. Units of nanoseconds
            /// </summary>
            public double time_1;
                        
            /// <summary>
            /// Time when particle detected at second detector. Units of nanoseconds
            /// </summary>
            public double time_2;

            /// <summary>
            /// Kinetic energy of decayed particle. Units of MeV
            /// </summary>
            public double energy;
        }

        
        static void Main(string[] args)
        {
            string filepath = @"C:\Users\Josh\Downloads\p3data7.dat";

            List<Event> events = ParseFile(filepath);

            // take some quick data on the events

            var goodEvents = (from item in events
                              where item.energy >= 0
                              select item)
                              .ToList();
            // get the positive energy events, store in a new List goodEvents

            var badEvents = from item in events
                            where item.energy < 0
                            select item;

            int numberOfBadEvents = badEvents.Count();

            double averageEvergy = (from item in goodEvents select item.energy).Average();

            double averageT2 = (from item in goodEvents select item.time_2).Average();

            Console.WriteLine("Number of bad events: " + numberOfBadEvents);
            Console.WriteLine("Average energies" + averageEvergy);
            Console.WriteLine("Average t2: " + averageT2);



        }

        /// <summary>
        /// Takes a file containing 2 times and an energy, and stores these values in a list
        /// </summary>
        /// <param name="filepath">Path to the file to be parsed</param>
        /// <returns>Returns all the Events as a List</returns>
        static List<Event> ParseFile(string filepath)
        {
            System.IO.StreamReader inputFile = null;

            var events = new List<Event>();
            // List of events

            try
            {
                inputFile = new System.IO.StreamReader(filepath);
            }

            catch (System.IO.FileNotFoundException ex)
            {
                // Put the more specific exception first.
                 // TODO: implement exceptions fully. Prompt user for filepath (default to wherever i'm saving them), and if it doesn't exist return and reprompt
                System.Console.WriteLine(ex.ToString());
            }

            string line;
            
            // read the full file line by line
            while ((line = inputFile.ReadLine()) != null)
            {
                // ignore header lines (lines beginning with '#' or blank lines
                // for some reason it has the line as "" and it's not equal to null and doesn't terminate so that's why there's the length thing idfk
                if ((line.Length == 0) || (line[0] == '#'))
                {
                    continue;
                }
                
                var thisEvent = new Event();
                // declare variable for the event we're reading off this line
                
                // use a regular expression to extract the 3 variables
                try
                {
                    thisEvent.time_1 = Convert.ToDouble(Regex.Match(line, @"(\d*.?\d*)(?=\st2\s=)").ToString());
                    thisEvent.time_2 = Convert.ToDouble(Regex.Match(line, @"(\d*.?\d*)(?=\sE\s=)").ToString());
                    thisEvent.energy = Convert.ToDouble(Regex.Match(line, @"(-?\d*.?\d*)$").ToString());

                    // TODO: modify the regex to handle malformed files

                    // TODO: (optional) figure out how long the regex is taking and maybe use RegexOptions.Compiled isntead: http://www.dotnetperls.com/regexoptions-compiled
                }
                catch (Exception)
                {
                    // throw exception if values don't cast to floats
                    // write the errant values and line number here 
                    // Error on line xxx:
                    // t1 = xxx t2 = e = 
                    // <line>
                    throw;
                }

                events.Add(thisEvent);
                // append this event
            }

            return events;
            // should return a fully populated list
        }
    }
}
