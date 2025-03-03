﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PakizaSoft_Assignment_Fahad.Models;
using PakizaSoft_Assignment_Fahad.ViewModels;
using System.Security.Cryptography;

namespace PakizaSoft_Assignment_Fahad.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ProductDbContext db;
        public OrdersController(ProductDbContext db) 
        { 
            this.db = db;
        }
        public IActionResult Index()
        {
            var data = db.Orders
                .Include(x=> x.Customer)
                .Include(x=> x.OrderItems).ThenInclude(y=> y.Product)
                .ToList();
            return View(data);
        }
        public ActionResult Create()
        {
            ViewBag.CustomerNames = db.Customers.Select(X=> X.CustomerName).ToList();
            ViewBag.ProductNames = db.Products.Select(X=> X.ProductName).ToList();
            ViewBag.OrderNo = db.Orders.OrderBy(y => y.OrderNo).Select(x => x.OrderNo).Last();

            return View();
        }
        [HttpPost]
        public JsonResult Create(Customer customer)
        {
           
            if (ModelState.IsValid)
            {
                var order = new Order { OrderDate=DateTime.Now};
                var existing = db.Customers.AsEnumerable().FirstOrDefault(x => string.Equals(customer.CustomerName, x.CustomerName, StringComparison.InvariantCultureIgnoreCase));
                if(existing == null)
                {
                    db.Customers.Add(customer);
                    order.Customer = customer;
                }
                else
                {
                    existing.Phone = customer.Phone;
                    existing.Address = customer.Address;
                    order.Customer = existing;
                }
               

                var itemData = HttpContext.Session.GetString("orderitems");
                if (itemData !=null)
                {
                    var orderItems = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(itemData);
                    if (orderItems != null)
                    {
                        foreach (var item in orderItems)
                        {
                            var prod = db.Products.FirstOrDefault(x => x.ProductName.ToLower()==item.ProductName.ToLower());
                            if (prod == null) { 
                                var newP = new Product { ProductName=item.ProductName, UnitPrice=item.UnitPrice};
                                db.Products.Add(newP);
                                order.OrderItems.Add(new OrderItem { Product=newP, Quantity = item.Quantity, UnitPrice=item.UnitPrice });
                            }
                            else
                            {
                                order.OrderItems.Add(new OrderItem { Product = prod, Quantity = item.Quantity });
                            }
                           
                        }
                    }
                    
                }
                db.Orders.Add(order);
                db.SaveChanges();
                HttpContext.Session.Remove("orderitems");
                return Json(new {success= true});

            }
            return Json(new {success=false});
        }
        [HttpPost]
        public PartialViewResult AddItem(OrderItemViewModel? item=null)
        {
            var itemData = HttpContext.Session.GetString("orderitems");
            List<OrderItemViewModel> items = new List<OrderItemViewModel>();
            if (itemData != null)
            {
                var x = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(itemData);
                if (x != null) { 
                    items = x; 
                }
                
            }
            if (item != null && !string.IsNullOrEmpty(item.ProductName))
            {
                items.Add(item);
                HttpContext.Session.SetString("orderitems", JsonConvert.SerializeObject(items));
                return PartialView("_ItemList", items);
            }
            else
            {
                if (items.Count > 0)
                {
                    return PartialView("_ItemList", items);
                }
            }
            return PartialView("_ItemList", null);
        }
        [HttpPost]
        public PartialViewResult UpdateItem(OrderItemViewModel item)
        {
            var itemData = HttpContext.Session.GetString("orderitems");
            List<OrderItemViewModel> items = new List<OrderItemViewModel>();
            if (itemData != null)
            {
                var x = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(itemData);
                if (x != null) { items = x; }

            }
            var existing = items.FirstOrDefault(x=> x.UniqueId == item.UniqueId);
            if(existing != null)
            {
                items.Remove(existing);
                items.Add   (item);
                HttpContext.Session.SetString("orderitems", JsonConvert.SerializeObject(items));
            }
            return PartialView("_ProductAddForm");
        }
        [HttpPost]
        public PartialViewResult EditItem(string uniqueId)
        {
            var itemData = HttpContext.Session.GetString("orderitems");
            List<OrderItemViewModel> items = new List<OrderItemViewModel>();
            if (itemData != null)
            {
                var x = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(itemData);
                if (x != null) { items = x; }

            }
           var item =items.FirstOrDefault(x=>  x.UniqueId == uniqueId);
            if(item != null)
            {
                return PartialView("_ProductEditForm", item);
            }
            return PartialView("_ProductAddForm");
        }
        [HttpPost]
        public PartialViewResult DeleteItem(string uniqueId)
        {
            var itemData = HttpContext.Session.GetString("orderitems");
            List<OrderItemViewModel> items = new List<OrderItemViewModel>();
            if (itemData != null)
            {
                var x = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(itemData);
                if (x != null) { items = x; }

            }
            var existing = items.FirstOrDefault(x => x.UniqueId == uniqueId);
            if (existing != null)
            {
                items.Remove(existing);
                HttpContext.Session.SetString("orderitems", JsonConvert.SerializeObject(items));
            }
            if(items.Count > 0)
            {
                return PartialView("_ItemList", items);
            }
            else
            {
                return PartialView("_ItemList", null);
            }
           
        }
        public PartialViewResult AddItemForm()
        {
            return PartialView("_ProductAddForm");
        }
        public JsonResult GetCustomerDetail(string customerName) 
        { 
            var customer = db.Customers.AsEnumerable().FirstOrDefault( x => string.Equals(customerName, x.CustomerName, StringComparison.InvariantCultureIgnoreCase));
            
            return Json(customer);
        }
        public JsonResult GetProductDetail(string productName)
        {
            var product = db.Products.AsEnumerable().FirstOrDefault(x => string.Equals(productName, x.ProductName, StringComparison.InvariantCultureIgnoreCase));

            return Json(product);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var o = await db.Orders.Include(y => y.Customer).Include(x => x.OrderItems).ThenInclude(z => z.Product).FirstOrDefaultAsync(x => x.OrderId == id);
            if (o == null)
            {
                return NotFound();
            }
            var model = new OrderInputModel
            {
                OrderId = id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.CustomerName,
                Phone = o.Customer.Phone,
                Address = o.Customer.Address,
                OrderItems = o.OrderItems.ToList()
            };


            ViewBag.CustomerNames = db.Customers.Select(X => X.CustomerName).ToList();
            ViewBag.ProductNames = db.Products.Select(X => X.ProductName).ToList();
            ViewBag.OrderNo = db.Orders.OrderBy(y => y.OrderNo).Select(x => x.OrderNo).Last();
            foreach (var item in o.OrderItems)
            {
                ViewBag.ProductId = new SelectList(db.Products, "ProductId", "ProductName", item.ProductId);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(OrderInputModel model, string act = "")
        {
            var o = await db.Orders.Include(y => y.Customer).Include(x => x.OrderItems).ThenInclude(z => z.Product).FirstOrDefaultAsync(x => x.OrderId == model.OrderId);
            if (o == null)
            {
                return NotFound();
            }


            if (act == "add")
            {
                model.OrderItems.Add(new OrderItem());
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.RawValue = null;
                }
            }
            if (act.StartsWith("remove"))
            {
                int index = int.Parse(act.Substring(act.IndexOf("_") + 1));
                model.OrderItems.RemoveAt(index);
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.RawValue = null;
                }
            }
            if (act == "update")
            {

                o.Customer.CustomerName = model.CustomerName;
                o.Customer.Phone = model.Phone;
                o.Customer.Address = model.Address;
                db.OrderItems.RemoveRange(o.OrderItems.ToList());
                o.OrderItems.Clear();
                foreach (var oi in model.OrderItems)
                {
                    o.OrderItems.Add(new OrderItem { OrderId = o.OrderId, Quantity = oi.Quantity, Product = oi.Product, ProductId = oi.ProductId });
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerNames = db.Customers.Select(X => X.CustomerName).ToList();
            ViewBag.ProductNames = db.Products.Select(X => X.ProductName).ToList();
            ViewBag.OrderNo = db.Orders.OrderBy(y => y.OrderNo).Select(x => x.OrderNo).Last();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = new Order { OrderId = id };
            db.Entry(order).State = EntityState.Deleted;
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return Json(new { success = true, msg = "Data deleted" });
        }
        public IActionResult Print(int id)
        {
            var data = db.Orders.Include(y => y.Customer).Include(x => x.OrderItems).ThenInclude(z => z.Product).FirstOrDefault(o => o.OrderId == id);
            return View(data);
        }

    }
}
