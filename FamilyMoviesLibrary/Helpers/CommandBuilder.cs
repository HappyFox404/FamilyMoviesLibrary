﻿namespace FamilyMoviesLibrary.Services.Helpers;

public class CommandBuilder
{
    public const string ContinueKey = "-c:";
    public readonly string RawCommand;
    public string Command { get; }
    public List<string> Arguments { get; }
    public bool ValidCommand { get; }

    public CommandBuilder(string command)
    {
        RawCommand = command;
        if (command.Contains("\"") == false)
        {
            var values = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (values.Any())
            {
                Command = values.First();
                if (Command.Length > 1)
                {
                    Arguments = values.Skip(1).ToList();
                }
                ValidCommand = true;
            }
            else
            {
                ValidCommand = false;
            }
        }
        else
        {
            bool isMark = false;
            List<string> elements = new List<string>();
            string temp = String.Empty;
            foreach (var symbol in command.ToCharArray())
            {
                if (isMark == false)
                {
                    if (symbol == ' ')
                    {
                        elements.Add(temp);
                        temp = String.Empty;
                    } 
                    else if (symbol == '"')
                    {
                        isMark = true;
                    }
                    else
                    {
                        temp+=symbol;
                    }
                }
                else
                {
                    if (symbol == '"')
                    {
                        isMark = false;
                        elements.Add(temp);
                        temp = String.Empty;
                    }
                    else
                    {
                        temp+=symbol;
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(temp) == false)
            {
                elements.Add(temp);
            }

            elements = elements.Where(x => !string.IsNullOrEmpty(x)).ToList();

            if (elements.Any())
            {
                Command = elements.First();
                if (Command.Length > 1)
                {
                    Arguments = elements.Skip(1).ToList();
                }
                ValidCommand = true;
            }
            else
            {
                ValidCommand = false;
            }
        }
    }

    public string? GetArgumentValue(string argument)
    {
        foreach (var arg in Arguments)
        {
            if (arg.Contains($"-{argument}:"))
            {
                return arg.Replace($"-{argument}:", "");
            }
        }

        return null;
    }
    
    public string GetContinueValue()
    {
        foreach (var arg in Arguments)
        {
            if (arg.Contains(ContinueKey))
            {
                return arg.Replace(ContinueKey, "");
            }
        }
        throw new ArgumentOutOfRangeException($"Не найден ключ продолжения");
    }

    public bool ContainsArgumentKey(string key)
    {
        return RawCommand.Contains(key);
    }

    public bool ContainsContinueKey()
    {
        return RawCommand.Contains(ContinueKey);
    }
}