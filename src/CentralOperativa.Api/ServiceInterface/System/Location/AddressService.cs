using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using Address = CentralOperativa.ServiceModel.System.Location.Address;

namespace CentralOperativa.ServiceInterface.System.Location
{
    [Authenticate]
    public class AddressService : ApplicationService
    {
        private readonly PlaceService placeService;

        public IAutoQueryDb AutoQuery { get; set; }

        public AddressService(PlaceService placeService)
        {
            this.placeService = placeService;
        }

        public Address.GetAddressResult Get(Address.Get request)
        {
            var model = Db.LoadSingleById<Domain.System.Location.Address>(request.Id).ConvertTo<Address.GetAddressResult>();
            model.Place = placeService.Get(new ServiceModel.System.Location.Place.Get { Id = model.PlaceId });
            return model;
        }

        
        public object Any(Address.Query request)
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, query);
        }



        public object Put(Address.Post request)
        {
            var addr = Db.LoadSingleById<Domain.System.Location.Address>(request.Id);

           addr.PopulateWith(request);

           Db.Update<PersonAddress>(new {
               TypeId = request.TypeId,
               TypeName = request.TypeName
           }, @where: c => c.AddressId == request.Id);

            Db.Save(addr);
            return request.ConvertTo<Domain.System.Location.Address>();
        }


        public Domain.System.Location.Address Post(Address.Post request)
        {
            var address = new Domain.System.Location.Address
            {
                Floor = request.Floor,
                Street = request.Street,
                StreetNumber= request.StreetNumber,
                Appartment = request.Appartment,
                PlaceId = 3 // corregir con Pablo
            };

            address.Id = (int)Db.Insert(address, true);

            var personadress = new PersonAddress
            {
                PersonId = request.PersonId,
                AddressId = address.Id,
                TypeId = request.TypeId
            };

            if (request.TypeId == 1)
            {
                personadress.TypeName = request.TypeName;
            }

            Db.Insert(personadress, false);

            return address;

        }

        public object Delete(Address.Delete request)
        {
            var addressId = Db.From<PersonAddress>().Select(c => c.Id).Where(c => c.AddressId == request.Id);
            Db.Delete<PersonAddress>(addressId);
            return Db.DeleteById<Domain.System.Location.Address>(request.Id);
        }
    }
}
