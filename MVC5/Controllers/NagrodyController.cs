using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MVC5.Models;
using MVC5.ViewModels;

namespace MVC5.Controllers
{
    public class NagrodyController : Controller
    {
        private NagrodyContext db = new NagrodyContext();

        // GET: /Nagrody/
        public ActionResult Index()
        {
            return View(db.NagrodyModels.ToList());
        }

        // GET: /Nagrody/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NagrodyModel nagrodymodels = db.NagrodyModels.Find(id);
            if (nagrodymodels == null)
            {
                return HttpNotFound();
            }
            return View(nagrodymodels);
        }

        // GET: /Nagrody/Create
        public ActionResult Create()
        {
            return View();
        }

        // akcja przekazujaca id wybranej przez Nas nagordy
        // z widoku uzytkownika
        public ActionResult LosujWybranaNagrode(int? id)
        {
            // zapisujemy sobie to ID do zmiennej sesyjnej
            Session["IdProduktu"] = id;
            // przechodzimy do akcji w innym kontrolerze
            return RedirectToAction("LosujWybranaNagrode", "Losowanie");
        }

        // POST: /Nagrody/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NagrodyViewModel nagrodymodels)
        {
            if (ModelState.IsValid)
            {
                string pathToBase = "";
                if (nagrodymodels.Obrazek != null)
                {
                    var fileName = Path.GetFileName(nagrodymodels.Obrazek.FileName);
                    pathToBase = "/Content/Images/" + fileName;
                    

                    // dopuszczalne rozszerzenia dla obrazka
                    var allwedExtensions = new string[]
                    {
                        "jpg", "Jpg", "jpeg", "Jpeg", "png", "Png"
                    };
                    // wgrywamy plik na serwer
                    var path = Path.Combine(Server.MapPath("/Content/Images"), fileName);
                    // tniemy nazwe pliku na kropce
                    var uploadedfileExtension = fileName.Split('.', ' ');
                    // zapisujemy same rozszerzenie
                    var extension = uploadedfileExtension[1].Trim();
                    // dodano walidacje rozszerzenia wgrywanego pliku
                    if (!allwedExtensions.Contains(extension))
                    {
                        return View("ZlyFormatPlikuError");
                    }
                    nagrodymodels.Obrazek.SaveAs(path);
                }
                var nagrodyToBase = new NagrodyModel
                {
                    Tytul = nagrodymodels.Tytul,
                    Cena = nagrodymodels.Cena,
                    Obrazek = pathToBase, //ścieżka do pliku w katalogu Images
                    Opis = nagrodymodels.Opis
                };

                db.NagrodyModels.Add(nagrodyToBase);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(nagrodymodels);
        }

        // GET: /Nagrody/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NagrodyModel nagrodymodels = db.NagrodyModels.Find(id);
            if (nagrodymodels == null)
            {
                return HttpNotFound();
            }
            return View(nagrodymodels);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ID,Tytul,Opis,Cena,Obrazek")] NagrodyModel nagrodymodels, NagrodyViewModel nagrodyViewModel)
        {
            if (ModelState.IsValid)
            {
                string pathToBase = "";
                // upload nowego obrazka po edycji na serwer
                // do katalogu Content/Images
                if (nagrodymodels.Obrazek != null)
                {
                    var fileName = Path.GetFileName(nagrodyViewModel.Obrazek.FileName);
                    pathToBase = "/Content/Images/" + fileName;
                    
                    // dodano walidacje rozszerzenia wgrywanego pliku
                    // dopuszczalne rozszerzenia dla obrazka
                    var allwedExtensions = new string[]
                    {
                        "jpg", "Jpg", "jpeg", "Jpeg", "png", "Png"
                    };
                    // wgrywamy plik na server
                    var path = Path.Combine(Server.MapPath("/Content/Images"), fileName);
                    // tniemy nazwe pliku na kropce
                    var uploadedfileExtension = fileName.Split('.', ' ');
                    // zapisujemy same rozszerzenie
                    var extension = uploadedfileExtension[1].Trim();
                    // dodano walidacje rozszerzenia wgrywanego pliku
                    if (!allwedExtensions.Contains(extension))
                    {
                        return View("ZlyFormatPlikuError");
                    }
                    nagrodyViewModel.Obrazek.SaveAs(path);
                }
                var nagrodyToBase = new NagrodyModel
                {
                    Tytul = nagrodymodels.Tytul,
                    Cena = nagrodymodels.Cena,
                    Obrazek = pathToBase, //ścieżka do pliku w katalogu Images
                    Opis = nagrodymodels.Opis
                };
                // przepisywanie sciezki po edycji
                nagrodymodels.Obrazek = nagrodyToBase.Obrazek;

                // zaincjalizowanie zmian dla obiektu
                db.Entry(nagrodymodels).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nagrodymodels);
        }

        // GET: /Nagrody/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NagrodyModel nagrodymodels = db.NagrodyModels.Find(id);
            if (nagrodymodels == null)
            {
                return HttpNotFound();
            }
            return View(nagrodymodels);
        }

        // POST: /Nagrody/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NagrodyModel nagrodymodels = db.NagrodyModels.Find(id);
            db.NagrodyModels.Remove(nagrodymodels);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
