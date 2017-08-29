namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using System.Threading;
    using Constents;
    using System.Dynamic;
    using Infrastructure.Data.Umbraco.Repositories;
    using Core.Domain.Repositories;
    using System.Linq;

    public class ProfileController : BaseController
    {
        #region Fields

        // GET: Profil
        private readonly OodsServices accountService;
        private readonly IRegistrationPetsRepository registrationPetrepo;
        private readonly IProfileSettingsRepository profileSettingsRepository;
        private readonly ILoginRepository loginRepository;

        #endregion

        #region Constructors & Destructors

        public ProfileController(ILoginRepository loginRepositoryParam,IProfileSettingsRepository profileSettingsRepositoryParam,IRegistrationPetsRepository registrationPetrepo,OodsServices accountServiceParam, IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
            accountService = accountServiceParam;
            this.registrationPetrepo = registrationPetrepo;
            loginRepository = loginRepositoryParam;
            profileSettingsRepository = profileSettingsRepositoryParam;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            if (!Authentication.isLogin())
            {
                return Redirect("/");
            }
            Session["culture"] = Thread.CurrentThread.CurrentUICulture;
            //var user = accountService.SearchContact(Authentication.GetUsername(), "GB_2660_0");
            var user = accountService.GetUser(Int32.Parse(Authentication.GetConsumerId()));

            var petList = new List<PetsModel>();
            if(user.pets != null && user.pets.Length > 0)
            {
                foreach (var pt in user.pets)
                {
                    var pet = new PetsModel
                    {
                        petType = pt.PET_TYPE_CODE.ToString(),
                        petName = pt.PET_NAME,
                        petGender = pt.PET_GENDER_CODE.Equals(2) ? "Male" : "Female",
                        petBirthDate = pt.PET_BIRTHDATE.ToString("d"),
                        PetBreedId = pt.PET_BREED_CODE,
                        petWeigh = SiteParameters.getPetWeightCrmToForm(pt.PET_SIZE_CODE),
                        petCoat = SiteParameters.getPetCoatCrmToForm(pt.PET_COAT_LENGTH),
                        petActive = pt.PET_ACTIVITY_LEVEL,
                        isPetSterilized = pt.PET_NEUTERING_STATUS_CODE.Equals(1) ? "Yes" : "No",
                        petAloneSitter = pt.PET_ALONE_TIME_CODE,
                        //isProductTriedBefore = pt.PET_TRIED_LIGHT_FOOD.Equals(1) ? "Yes" : "No",
                        petBirthDay = pt.PET_BIRTHDATE.Day,
                        petBirthMonth = pt.PET_BIRTHDATE.Month,
                        petBirthYear = pt.PET_BIRTHDATE.Year,
                        petPhotos = pt.PET_PHOTO_URL,
                        petSpendTime = pt.PET_ACCOMODATION_CODE.ToString(),
                        petId = pt.PET_ID,
                        petAgeIndex = SiteParameters.getSliderUpdateProfileIndex(pt.PET_BIRTHDATE)
                        
                    };
                    if (!pt.PET_TRIED_LIGHT_FOOD.Equals(0))
                    {
                        pet.isProductTriedBefore = pt.PET_TRIED_LIGHT_FOOD.Equals(1) ? "Yes" : "No";
                    }else
                    {
                        pet.isProductTriedBefore = "unknown";
                    }
                    petList.Add(pet);
                }
            }

            var bannerId = CurrentPage.HasProperty("banner") ? CurrentPage.GetPropertyValue<int>("banner") : 0;

            var media = Umbraco.TypedMedia(bannerId);

            
            var register = new RegisterModel
            {
                FirstName = user.consumer.FIRSTNAME,
                LastName = user.consumer.SURNAME,
                Address1 = user.consumer.STREET_NAME,
                Address2 = user.consumer.STREET_ADDRESS,
                Email = user.login.username,
                City = user.consumer.CITY,
                PostalCode = user.consumer.POSTAL_CODE,
                BirthDate = user.consumer.BIRTHDATE.ToString("d")
            };

            var optPerfectFitBrandId = 278; //TO DO : to place in umbraco parameter

            register.MyProfileQuestion = "no";
            register.UpdateMarsQuestion = "no";

            if (user.brandPreferences != null && user.brandPreferences.Length > 0)
            {
                for (int i = 0; i < user.brandPreferences.Length; i++)
                {
                    if (user.brandPreferences[i].BP_BRAND_ID.Equals(optPerfectFitBrandId))
                    {
                        register.MyProfileQuestion = "yes";
                    }
                }
            }

            if (user.marketingPermissions != null && user.marketingPermissions.Length > 0)
            {
                for(int i = 0; i < user.marketingPermissions.Length; i++)
                {
                    if(user.marketingPermissions[i].OPT_IN_BRAND_ID.Equals(optPerfectFitBrandId))
                    {
                        register.UpdateMarsQuestion = "yes";
                    }
                }
            }

            var model = new ProfilViewModel
            {
                Registration = register,
                Banner = ImageHelper.GetImage(media),
                Pets = petList,
                ReceiveMarketingEmail = profileSettingsRepository.GetReceiveMarketingEmail(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString())
            };


            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));

            Dictionary<string, string> allBreed = registrationPetrepo.getAllBreedstring(cultureBydom.ToString());

            ViewBag.AllBreeds = allBreed;

            ViewBag.DataLayer = GenerateProfileTracking();

            return View(model);
        }


        private string GenerateProfileTracking()
        {
            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "Profile";
            if (Session["user_profile_dl"] != null)
            {
                datalayer.MetaDataUserProfile = Session["user_profile_dl"];
            }
            var layer = this.GenerateCustomVariable(datalayer);
            return layer;
        }



        #endregion
    }
}
