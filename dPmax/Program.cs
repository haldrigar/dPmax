using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using Catfood.Shapefile;

namespace dPmax
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("\nProgram do oblicznia dopuszczalnej odchyłki dla działek wg Rozporządzenia z dnia 9 listopada 2011\n");
                Console.WriteLine("Uruchom progam z parametrami w postaci nazwy pliku SHP oraz opcjonalnie z wartoscia parametru 'mp'");
                Console.WriteLine("Jeśli nie podasz parametru 'mp' zostanie mu przypisana wartość mp = 0.1\n");
                Console.WriteLine("dpmax.exe nazwa_pliku [mp]");
                Console.ReadKey();
                return;
            }

            StreamWriter fileOut;

            try
            {
                fileOut = new StreamWriter(new FileStream("c:\\temp\\dzd.txt", FileMode.Create), Encoding.GetEncoding(1250));
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            fileOut.WriteLine("{0}\t{1}\t{2}", "[numerDzialki]", "[powierzchniaCalkowita]", "[dPmaxWynikowe]");

            try
            {
                using (Shapefile shapefile = new Shapefile(args[0]))
                {

                    foreach (Shape shape in shapefile)
                    {

                        if (shape.Type == ShapeType.Polygon)
                        {
                            ShapePolygon shapePolygon = (ShapePolygon)shape;

                            //string[] wartosci = shape.GetMetadataNames();

                            string numerDzialki = shape.GetMetadata("Numer");

                            Console.WriteLine("dzialka: " + numerDzialki);
                            Console.WriteLine("liczba czesci: " + shapePolygon.Parts.Count);

                            double powierzchniaCalkowita = 0;
                            double dPmaxWynikowe = 0;

                            foreach (PointD[] part in shapePolygon.Parts)
                            {

                                int liczbaPunktow = part.Length;

                                Console.WriteLine("liczba punktow czesci: " + liczbaPunktow);

                                PointD[] punkty = new PointD[liczbaPunktow + 1];

                                punkty[0] = part[liczbaPunktow - 2];

                                for (int i = 0; i < liczbaPunktow; i++)
                                {
                                    //fileOut.WriteLine(part[i].Y + " " + part[i].X);
                                    punkty[i + 1] = part[i];
                                }

                                double powierzchnia = 0;

                                for (int i = 0; i < liczbaPunktow - 1; i++)
                                {
                                    powierzchnia = powierzchnia + (part[i].Y + part[i + 1].Y) * (part[i + 1].X - part[i].X);
                                }

                                Console.WriteLine("powierzchnia czesci: " + Math.Round(powierzchnia, 2));

                                powierzchniaCalkowita = powierzchniaCalkowita + (powierzchnia / 2);

                                double suma = 0;

                                for (int i = 1; i < liczbaPunktow; i++)
                                {
                                    suma = suma + Math.Pow(punkty[i - 1].X - punkty[i + 1].X, 2) + Math.Pow(punkty[i - 1].Y - punkty[i + 1].Y, 2);
                                }

                                double mp = 0.1;

                                if (args.Length > 1 && args[1] != null)
                                {
                                    mp = Convert.ToDouble(args[1]);
                                }

                                double dpMax = Math.Sqrt(suma / 8) * mp;

                                if (dpMax > dPmaxWynikowe)
                                {
                                    dPmaxWynikowe = dpMax;
                                }

                                Console.WriteLine("dpMax czesci: {0}", Math.Round(dpMax, 2));
                            }

                            Console.WriteLine("powierzchnia calkowita: {0}", Math.Round(powierzchniaCalkowita, 2));
                            Console.WriteLine("dPmax wynikowe: {0}\n", Math.Round(dPmaxWynikowe, 2));

                            fileOut.WriteLine("{0}\t{1}\t{2}", numerDzialki, Math.Round(powierzchniaCalkowita, 2), Math.Round(dPmaxWynikowe, 2));

                        }
                        else
                        {
                            Console.WriteLine("To nie jest plik typu Polygon");
                        }

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }


            fileOut.Close();

            Console.ReadKey();
        }
    }
}
