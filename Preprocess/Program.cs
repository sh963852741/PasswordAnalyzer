// See https://aka.ms/new-console-template for more information

double trainSize = 0.6;
StreamReader sr;
StreamWriter sw;
StreamWriter sw_train;
StreamWriter sw_test;
try
{
    sr = File.OpenText("csdn_password.txt");
    sw = new StreamWriter(File.OpenWrite("filtered_csdn_password.txt"));
}
catch
{
    return;
}

int totalLines = 0;
while(!sr.EndOfStream)
{
    string? line = sr.ReadLine();
    if (line == null) continue;

    string[] parts = line.Split(" # ");
    if (parts.Length != 3)
    {
        throw new NotImplementedException();
    }
    else
    {
        sw.WriteLine(parts[1]);
    }
    ++totalLines;
}
sr.Close();
sw.Close();

int trainLines = (int)(totalLines * trainSize);
int testLines = totalLines - trainLines;
Console.WriteLine($"{totalLines}{trainLines}{testLines}");
sr = File.OpenText("filtered_csdn_password.txt");
sw_train = new StreamWriter(File.OpenWrite("csdn_train_password.txt"));
sw_test = new StreamWriter(File.OpenWrite("csdn_test_password.txt"));

for(int i = 0; i < trainLines; ++i)
{
    sw_train.WriteLine(sr.ReadLine());
}
sw_train.Close();

for (int i = 0; i < testLines; ++i)
{
    sw_test.WriteLine(sr.ReadLine());
}
sw_test.Close();

sr.Close ();