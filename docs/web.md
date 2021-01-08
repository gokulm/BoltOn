**BoltOn.Web** has a few filters and middlewares to be used in MVC/WebAPI applications.

Filters
-------
* [`ModelValidationFilter`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Web/Filters/ModelValidationFilter.cs)
<br />
It's a global filter which validates the models and throws BadRequest 400 HTTP response with the validation error messages set in the DataAnnotation attributes.

* [`CustomExceptionFilter`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Web/Filters/CustomExceptionFilter.cs)
<br />
It's a global filter to handle all the exceptions thrown in the application and returns any of these HTTP responses:
    * HTTP Response 400 - This is returned when a [`BadRequestException`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Web/Exceptions/BadRequestException.cs) is thrown in the application, which usually happens for model validation errors. The ModelValidationFilter mentioned above throws this exception.
    * HTTP Response 412 - This is returned when a [`BusinessValidationException`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Web/Exceptions/BusinessValidationException.cs) is thrown in the application, which is mainly used to display the actual business validation message to the user.
    * HTTP Response 500 - This status code is returned for all the unhandled exceptions with a generic error message or the actual exception message. The generic error message can be set in the appSettings with the key "ErrorMessage", and whether to show the actual exception message or a generic message can be controlled by a flag with the key "IsShowErrors" in the appSettings. It's recommened to enable IsShowErrors in the lower environments and disable it in the higher environments as the exception messages could have sensitive information.


The global filters can be registered like this in a WebAPI application:

    services.AddControllers(c =>
    {
        c.Filters.Add<CustomExceptionFilter>();
        c.Filters.Add<ModelValidationFilter>();
    });

For MVC application, `AddControllersWithViews` method can be used.

