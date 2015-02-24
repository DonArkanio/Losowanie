using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using MVC5.Models;
using MVC5.ViewModels;

namespace MVC5.Controllers
{
    public class LosowanieController : Controller
    {
        List<NagrodyModel> _kolekcjaNagrod = new List<NagrodyModel>(); 
        private NagrodyContext Context = new NagrodyContext();

        // GET: Losowanie
        
        public ActionResult Index()
        {
            return View();
        }

        

        //int zakrPoczatek, int zakrKoniec
        [HttpPost]
        public ActionResult Losuj(int? zakrPoczatek, int? zakrKoniec)
        {
            var zakresobj = new ZakresModel
            {
                Poczatek = zakrKoniec, 
                Koniec = zakrKoniec
            };

            // w przypadku gdy nie podamy zadnych wartosci
            if (zakresobj.Poczatek == null || zakresobj.Koniec == null )
            {
                return View("ZakresError");
            }

            // kolekcja zawierajaca wszystkie ID produktow w bazie
            var listaIDkow = new List<int>();
            using (var context = new NagrodyContext())
            {
                // tutaj pobieramy wszystkie produkty
                var tmp = context.NagrodyModels.ToList();

                // nastepnie dodajemy ich ID kolejno do kolekcji
                foreach (var nag in tmp)
                {
                    listaIDkow.Add(nag.ID);
                }
                
            }

            // jesli podane przez uzytkownika ID nie bedzie zgodne z ID produktu w bazie to
            // zwroci komunikat bledu
            if (!listaIDkow.Contains(zakresobj.Poczatek.Value) || !listaIDkow.Contains(zakresobj.Koniec.Value))
            {
                return View("BrakIdError");
            }

            if (ModelState.IsValid)
            {
                var wylosowana = LosujNagrodeZPodanegoZakresu(zakresobj.Poczatek, zakresobj.Koniec);
                return View(wylosowana);
            }
            return RedirectToAction("LosujNagrodeZPodanegoZakresu", "Losowanie");
        }

        string pathToCsv = System.Web.HttpContext.Current.Server.MapPath("~/Content/CSV/users.csv");

        // wczytywanie poszczegolnych lini z pliku CSV
        private static IEnumerable<string[]> ParsujCsv(string path)
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line.Split(',');
                }
            }
        }

        // poszczególne kolumny wyciagane z pliku CSV po przecinku
        // id , nazwa uzytkownika, adres email
        private const int IdColumn = 0;
        private const int UserNameColumn = 1;
        private const int EmailColumn = 2;
        //...

        // przypisywanie poszczególnych wartości do obiektu UserModel
        private UserModel ParsujUzytkownikow(string[] line)
        {
            return new UserModel
            {
                Id = int.Parse(line[IdColumn]),
                UserName = line[UserNameColumn],
                Email = line[EmailColumn],
            };
        }

        // akcja dla uzytkownikow wczytanych z pliku CSV
        private List<UserModel> _kolekcjaUzytkownikow;

        public List<UserModel> UzytkownicyZPlikuCsv()
        {
            // parsowanie pliku CSV linia po lini
            var kolekcjaLiniZPlikuCsv = ParsujCsv(pathToCsv);

            // tworzenie listy uzytkownikow po sparsowaniu
            _kolekcjaUzytkownikow = new List<UserModel>();

            // wyciagnie poszczegolnych danych uzytkownikow
            foreach (var usr in kolekcjaLiniZPlikuCsv)
            {
                // parsowanie uzytkownika
                var tmp = ParsujUzytkownikow(usr);
                // wycinanie ; na koncu
                if (tmp.Email.EndsWith(";"))
                {
                    // pozbywanie sie pustych spacji i niepotrzebnych znakow
                    var q = tmp.Email.TrimEnd(';');
                    // przepisywanie obrobionej wartosci
                    tmp.Email = q.Trim();
                }
                // dodawanie obrobionego uzytkownika do kolekcji
                _kolekcjaUzytkownikow.Add(tmp);
            }
            return _kolekcjaUzytkownikow;
        }

        public List<UserModel> UzytkownicyZPlikuCsv(string uploadedFilePath)
        {
            // parsowanie pliku CSV linia po lini
            var kolekcjaLiniZPlikuCsv = ParsujCsv(uploadedFilePath);

            // tworzenie listy uzytkownikow po sparsowaniu
            _kolekcjaUzytkownikow = new List<UserModel>();

            // wyciagnie poszczegolnych danych uzytkownikow
            foreach (var usr in kolekcjaLiniZPlikuCsv)
            {
                // parsowanie uzytkownika
                var tmp = ParsujUzytkownikow(usr);
                // wycinanie ; na koncu
                if (tmp.Email.EndsWith(";"))
                {
                    // pozbywanie sie pustych spacji i niepotrzebnych znakow
                    var q = tmp.Email.TrimEnd(';');
                    // przepisywanie obrobionej wartosci
                    tmp.Email = q.Trim();
                }
                // dodawanie obrobionego uzytkownika do kolekcji
                _kolekcjaUzytkownikow.Add(tmp);
            }
            return _kolekcjaUzytkownikow;
        }

        // odbieranie Id wybranej nagrody aby pozniej wylosować dla niej
        // jednego zwyciezce
        public ActionResult LosujWybranaNagrode()
        {
            // odbieranie Id z sesji i zapisanie do zmiennej
            var wybranyProduktId = (int?)Session["IdProduktu"];
            // pobieranie szczegolow tej nagrody po ID
            var daneNagrody = Context.NagrodyModels.Where(n => n.ID == wybranyProduktId);
            // pobieranie uzytkownikow z pliku CSV
            var allUsersList = UzytkownicyZPlikuCsv();

            //zapis wynikow losowania do OVM
            var ovm = new OstatecznyViewModel()
            {
                Nagroda = daneNagrody.FirstOrDefault(),
                Uzytkownik = LosujPojedynczegoUzytkownika(allUsersList)
            };

            // zwrocenie do widoku
            return View(ovm);
        }
        public ActionResult LosujDlaUzytkownikow()
        {
            var allUsersList = UzytkownicyZPlikuCsv();
            // przekazywanie tej kolekcji do Widoku
            return View(allUsersList);
        }

        // akcja odpowiedzialna za losowanie nagrody dla uzytkownika
        // z pliku CSV zuploadowanego z widoku
        public ActionResult LosujDlaZaladowanegoCsv()
        {
            return View();
        }

        private List<UserModel> _csvUsersList;
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LosujDlaZaladowanegoCsv(HttpPostedFileBase csvFile)
        {
            var fileName = Path.GetFileName(csvFile.FileName);
            // sprawdzanie rozszerzenia
            var allwedExtensions = new string[]
                    {
                        "csv", "CSV", "Csv"
                    };
            // wgrywamy plik na serwer
            var path = Path.Combine(Server.MapPath("~/Content/CSV"), fileName);
            // tniemy nazwe pliku na kropce
            var uploadedfileExtension = fileName.Split('.', ' ');
            // zapisujemy same rozszerzenie
            var extension = uploadedfileExtension[1].Trim();
            // dodano walidacje rozszerzenia wgrywanego pliku
            if (!allwedExtensions.Contains(extension))
            {
                return View("ZlyFormatPlikuError");
            }
            // zapisujemy plik w wybranej lokalizacji
            csvFile.SaveAs(path);

            // tutaj następuje ponowne parsowanie załadowanego pliku
            // dzielenie na poszczególne wiersze i zapis użytkowników
            // do kolekcji tymczasowej
            _csvUsersList = UzytkownicyZPlikuCsv(path);

            // wykorzystalem TempData[] aby przeniesc wartosc kolekcji z tej akcji
            TempData["kolekcjaUzytkownikow"] = _csvUsersList;
            return RedirectToAction("WyswietlWczytanychUzytkownikow","Losowanie");
        }

        // akcja sluzaca do wygenerowania widoku dla uzytkownikow
        // z wczytanego pliku CSV i wyswietlenie ich ewentualnego gravatara
        public ActionResult WyswietlWczytanychUzytkownikow()
        {
            var wczytaniUzytkownicy = (List<UserModel>)TempData["kolekcjaUzytkownikow"];
            TempData["wyswietleniUzytkownicy"] = wczytaniUzytkownicy;
            return View(wczytaniUzytkownicy);
        }

        // akcja ktora sluzy do zbierania zgromadzonych informacji o nagrodach
        // oraz wczytanych z pliku CSV uzytkownikach a nastepnie tworzy z nich 
        // ViewModel ktory bedzie stanowic wynik losowania i przekazuje je do
        // widoku koncowego
        public ActionResult LosujNagrodeDlaWczytanychUzytkownikow(OstatecznyViewModel ovm)
        {
            var wszystkieNagrody = Context.NagrodyModels.ToList();
            var wczytaniUserzy = (List<UserModel>)TempData["wyswietleniUzytkownicy"];

            ovm.Nagroda = LosujPojedynczaNagrode(wszystkieNagrody);
            ovm.Uzytkownik = LosujPojedynczegoUzytkownika(wczytaniUserzy);

            return View(ovm);
        }
      
        public ActionResult LosujJednaNagordeDlaJednegoUzytkownika(OstatecznyViewModel ovm)
        {
            // wyciagnie wszystkich nagrod z bazy
            var wszystkieNagrody = Context.NagrodyModels.ToList();
            // wyciaganie wszystkich uzytkownikow wczytanych zp liku CSV
            var wszyscyUzytkownicy = UzytkownicyZPlikuCsv();

            // losowanie jednego uzytkownika
            ovm.Uzytkownik = LosujPojedynczegoUzytkownika(wszyscyUzytkownicy);
            // losowanie jednej nagrody
            ovm.Nagroda = LosujPojedynczaNagrode(wszystkieNagrody);

            return View(ovm);
        }

        public UserModel LosujPojedynczegoUzytkownika(List<UserModel> usersList)
        {
            var rand = new Random();
            return usersList.ElementAt(rand.Next(usersList.Count));
        }

        
        public NagrodyModel LosujPojedynczaNagrode(List<NagrodyModel> nagrodyList )
        {
            var rand = new Random();
            //var wszystkieNagrody = Context.NagrodyModels.ToList();
            return nagrodyList.ElementAt(rand.Next(nagrodyList.Count));
        }

        

        public NagrodyModel LosujNagrodeZPodanegoZakresu(int? poczatek, int? koniec)
        {
            // wszystkie nagrody z bazy
            var dostepneNagrody = Context.NagrodyModels.ToList();

            // ID dostępnych nagrod z bazy
            var nagrodyIds = dostepneNagrody.Select(n => n.ID);

            // wszystkie dostępne nagrody z zadanego zakresu
            var nagrodyZZakresu = dostepneNagrody.Where(nagrody => nagrody.ID.Equals(poczatek) || nagrody.ID.Equals(koniec)).ToList();

            // losowanie 1 nagrody z zakresu podanych nagrod
            Random rand = new Random();
            var wylosowanaJedna = nagrodyZZakresu.ElementAt(rand.Next(nagrodyZZakresu.Count));

            return wylosowanaJedna;
        }
    }
}