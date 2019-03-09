﻿namespace AutoTagger.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AutoTagger.API.Models;
    using AutoTagger.Common;
    using AutoTagger.Contract;
    using AutoTagger.Contract.Models;

    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json;

    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private readonly ICustomerStorage customerStorage;

        public CustomerController(
            ICustomerStorage customerStorage
            )
        {
            this.customerStorage = customerStorage;
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(void), 200)]
        public IActionResult CreateCustomer()
        {
            try
            {
                var customer = new Customer();
                this.customerStorage.Create(customer);
                customer.GenerateHash();
                this.customerStorage.Update(customer);
                return this.Ok(customer.CustomerId);
            }
            catch (ArgumentException)
            {
                return this.NotFound();
            }
        }

    }
}