#include "pch.h"
#include <thread>
#include <iostream>
#include <string>
#include "Tools.h"

bool Inject(const char* procName, bool log = false)
{
	bool result = Inject(procName, "G:/SoftwareDev/C#/BotRelay/Debug/Mapping.dll");

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

		std::cout << "Waiting for process..." << std::endl;

		bool result;
		do
		{
			result = Inject(command.c_str());
			if (result)
			{
				std::cout << "Successfully injected!" << std::endl;
			}

		} while (!result);
	}

	return 0;
}