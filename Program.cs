using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer> 
{
    new Customer()
    {
        Id = 1,
        Name = "Debra Deter",
        Address = "2343 Bankoff Lane"
    },
    new Customer()
    {
        Id = 2,
        Name = "John Alex",
        Address = "5647 Hillside Road"
    },
    new Customer()
    {
        Id = 3,
        Name = "Bob Rune",
        Address = "9878 Fenley Drive"
    }
};
List<Employee> employees = new List<Employee> 
{
    new Employee()
    {
        Id = 1,
        Name = "Ed Warden",
        Specialty = "Being awesome"
    },
    new Employee()
    {
        Id = 2,
        Name = "Donna Paulson",
        Specialty = "Kicking ass"
    }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket> 
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "I'm having trouble with my purchase!",
        Emergency = false,
        DateCompleted = new DateTime(2024, 01, 12)
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Someone stole all my money!",
        Emergency = true,
        DateCompleted = new DateTime(2023, 02, 12)
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "I just need someone to talk to.",
        Emergency = false,
        DateCompleted = new DateTime(2021, 04, 12)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "I'm prepared to sue!",
        Emergency = true,
        DateCompleted = new DateTime()
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "I am requesting a refund!",
        Emergency = false,
        DateCompleted = new DateTime()
    },
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/servicetickets", () => 
{
    return serviceTickets;
});

app.MapGet("/serviceTickets/{id}", (int id) => 
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(s => s.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Customer = customers.FirstOrDefault(customers => customers.Id == serviceTicket.CustomerId);

    serviceTicket.Employee = employees.FirstOrDefault(employees => employees.Id == serviceTicket.EmployeeId);

    return Results.Ok(serviceTicket);
});

app.MapPut("/serviceTickets/{id}", (int id, ServiceTicket serviceTicket) => 
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapDelete("/serviceTickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.NoContent();
});

app.MapPost("/servicetickets/{id}/complete", (int id) => 
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);

    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/employees", () => 
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) => 
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) => 
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapPost("/serviceTickets", (ServiceTicket serviceTicket) => 
{
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});


app.Run();
