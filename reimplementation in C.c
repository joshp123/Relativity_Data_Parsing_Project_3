// reimplementing the data read and physics bits of project 3 in C b/c lol

#include <stdio.h>
#include <math.h>

// structure to hold the events b.c multiple arrays is for scrubs
typedef struct
{
	double time_1;
	double time_2;
	double kineticEnergy;
} Event;

// can i declare program wide constants here? lets find out. 

const int speedOfLight = 299792458;
const double positronRestMass = 0.5109989;
const int detectorSeparation = 3;


int CountFileLines(FILE *inputFile)
{
	int lineCount = 0;
	char line[256];
	while (fgets(line, sizeof line, inputFile) != NULL)
	{
		lineCount++;
	}
	return lineCount;
}

// needing 2 separate average functions b/c its a struct and c sucks. lol. this is literally be one line in c#:
// double averageEvergy = (from item in goodEvents select item.energy).Average();
// vs all this dumb garbage of multiple functions. lol just lol.

double EventsTime2Average(Event events[], int numberOfElements)
{
	double sum = 0;
	for (int i = 0; i < numberOfElements; ++i)
	{
		sum = sum + events[i].time_2;
	}
	double averageTime2 = sum / numberOfElements;
	return averageTime2;
}

double EventsKineticEnergyAverage(Event events[], int numberOfElements)
{
	double sum = 0;
	for (int i = 0; i < numberOfElements; ++i)
	{
		sum = sum + events[i].kineticEnergy;
	}
	double averageEnergy = sum / numberOfElements;
	return averageEnergy;
}

double EventsMaximumT2(Event events[], int numberOfElements)
{
	double max = events[0].time_2;
	for (int i = 0; i < numberOfElements; ++i)
	{
		if (events[i].time_2 > max)
		{
			max = events[i].time_2;
		}
	}
	return max;
}

double EventsMaximumKineticEnergy(Event events[], int numberOfElements)
{
	double max = events[0].kineticEnergy;
	for (int i = 0; i < numberOfElements; ++i)
	{
		if (events[i].kineticEnergy > max)
		{
			max = events[i].kineticEnergy;
		}
	}
	return max;
}


// lets copy and paste a bunch of C# code from the good project and then do tiny changes. ~*portability*~

double Beta(Event event)
{
    double velocity = detectorSeparation / ((event.time_2 - event.time_1) * pow(10, -9));
    return velocity / speedOfLight;
}


double Gamma(Event event)
{
    return 1 / (sqrt(1 - pow(Beta(event), 2)));
}

// just an aside but you literally have to order your functions here to get the compiler to be happy.
// yes, order matters in a _compiled_ (vs interpreted) language. lol.

double TotalEnergy(Event event)
{
    return event.kineticEnergy + positronRestMass;
}

double Momentum(Event event)
{
    return sqrt(pow(TotalEnergy(event), 2) - pow(positronRestMass, 2));
    // Square root of [ (energy^2) - m_0^2) ]
}

double TransformEnergy(Event event)
{
    return Gamma(event) * (TotalEnergy(event) - (Momentum(event) * Beta(event)));
}

double TransformTime(Event event)
{
	return event.time_2 / Gamma(event);
}

int main(int argc, char const *argv[])
{
	// step 1: read the file
	FILE *inputFile;
	inputFile = fopen("M:\\Code\\p3data7.dat", "r");
	int numberOfLines = CountFileLines(inputFile);
	rewind(inputFile);
	
	Event goodEvents[numberOfLines];
	// line var
	char line[256];

	int goodEventCount = 0;
	int badEventCount = 0;
	while (fgets(line, sizeof line, inputFile) != NULL)
	{
		Event thisEvent;

		if (line[0] == '#')
		{
			// header line, do nothing
		}
		else
		{
			int eventNumber;
			// assigning this variable inside the loop to read it in even though we don't actually care what the value is

			sscanf(line, "Event: %d t1 =  %lg t2 = %lg E =  %lg", &eventNumber, &thisEvent.time_1, &thisEvent.time_2, &thisEvent.kineticEnergy);
			// lol i'm legit surprised that you can even read into elements of a structure in c

			if (thisEvent.kineticEnergy >= 0)
			{
				goodEvents[goodEventCount] = thisEvent;
				goodEventCount++;
				// store val if energy is good
			}
			else
			{
				badEventCount++;
			}
		}
		

	}

	// file read complete, stats time
	printf("Step 1 statistics:\n");
	printf("Number of good events:\t%d\n", goodEventCount);
	printf("Number of bad events:\t%d\n", badEventCount);
	printf("Average Energy: \t%f\n", EventsKineticEnergyAverage(goodEvents, goodEventCount));
	printf("Average T2: \t\t%f\n", EventsTime2Average(goodEvents, goodEventCount));

	// relativity time yo

	// im too lazy to write a generic array function so instead of doing that
	// i'll make an array of Events where time1 = nothing, time2 = transformed time, kinetic energy = transformed energy
	// it's me im the coding horror

	// well actually doing basic standard library stuff using a specific function for each use case is the coding horror,
	// that's why we invented generics in *googles*, oh only 1973 literally forty years ago.
	// IEnumberable owns too and is also super useful but that's probably more recent than 1973 but still lmao

	Event transformedEvents[goodEventCount];

	for (int i = 0; i < goodEventCount; ++i)
	{
		transformedEvents[i].kineticEnergy = TransformEnergy(goodEvents[i]);
		// the kinetic energy is actually now the transformed energy. yes this variable naming is horrific
		transformedEvents[i].time_2 = TransformTime(goodEvents[i]);
		// similarly the time_2 is now the particle lifetime
	}

	printf("\nStep 2 statistics:\n");
	printf("Average transformed energy: \t%f\n", EventsKineticEnergyAverage(transformedEvents, goodEventCount));
	printf("Average particle lifetime: \t%f\n", EventsTime2Average(transformedEvents, goodEventCount));
	printf("Largest transformed energy: \t%f\n",EventsMaximumKineticEnergy(transformedEvents, goodEventCount));
	printf("Largest particle lifetime: \t%f\n",EventsMaximumT2(transformedEvents, goodEventCount));

	// if i really hated myself i'd reimplement histograms in C again except that's pretty awful really
	// since it's like 10 lines of LINQ in C# and it would be about 40 in c

	return 0;
}