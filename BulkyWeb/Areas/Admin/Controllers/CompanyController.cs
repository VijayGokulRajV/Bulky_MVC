using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       
        public CompanyController(IUnitOfWork unitOfWork )
        {
            _unitOfWork = unitOfWork;
           
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id) //upsert => update+insert
        {
           
            if (id == null || id == 0)
            {
                return View(new Company());

            }
            else
            {
                Company CompanyObj = _unitOfWork.Company.Get(item => item.Id == id );
                return View(CompanyObj);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {

            if (ModelState.IsValid)
            {

                //if create or add the company
                if(companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company Created Successfully...";
                return RedirectToAction("Index");
            }
            else
            {
              

                return View(companyObj);
            }

        }


        #region API calls

        [HttpGet]
        public IActionResult GetAll( )
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = objCompanyList });
        }


       // deleting the company
        public IActionResult Delete( int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(item => item.Id == id);

            if(companyToBeDeleted == null)
            {
                return Json(new { success = false , message = "Error while Deleting.."});
            }


            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();

                return Json(new { success = true , message = "Deleted sucessfully..."});

        }
        #endregion
    }
}
