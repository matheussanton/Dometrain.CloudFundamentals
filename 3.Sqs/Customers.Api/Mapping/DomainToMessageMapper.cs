﻿using Customers.Api.Contracts.Messages;
using Customers.Api.Domain;

namespace Customers.Api.Mapping
{
    public static class DomainToMessageMapper
    {
        public static CustomerCreated ToCustomerCreated(this Customer customer)
        {
            return new CustomerCreated
            {
                Id = customer.Id,
                GitHubUsername = customer.GitHubUsername,
                FullName = customer.FullName,
                Email = customer.Email,
                DateOfBirth = customer.DateOfBirth
            };
        }

        public static CustomerUpdated ToCustomerUpdated(this Customer customer)
        {
            return new CustomerUpdated
            {
                Id = customer.Id,
                GitHubUsername = customer.GitHubUsername,
                FullName = customer.FullName,
                Email = customer.Email,
                DateOfBirth = customer.DateOfBirth
            };
        }
    }
}
