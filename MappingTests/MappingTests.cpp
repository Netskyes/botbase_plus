// MappingTests.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Tools.h"
#include <thread>
#include <iostream>
#include <string>

bool Inject(bool log = false)
{
	bool result = Inject("ygopro_vs_links.exe", "G:/SoftwareDev/C#/BotRelay/Debug/Mapping.dll");

	if (log)
	{
		if (result)
		{
			std::cout << "Successfully injected!" << std::endl;
		}
		else
		{
			std::cout << "Failed to inject!" << std::endl;
		}
	}
	
	return result;
}

int main()
{
	while (true)
	{
		std::string command;
		std::cout << "> ";
		std::cin >> command;

		if (command == "i")
		{
			Inject(true);
		}
		else if (command == "iloop")
		{
			std::cout << "Waiting for process..." << std::endl;

			bool result;
			do 
			{
				result = Inject();
				if (result)
				{
					std::cout << "Successfully injected!" << std::endl;
				}

			} while (!result);
		}

	}

    return 0;
}


