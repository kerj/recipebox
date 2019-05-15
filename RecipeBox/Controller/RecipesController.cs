using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using RecipeBox.Models;


namespace RecipeBox.Controllers
{
  public class RecipesController : Controller
  {

    [HttpGet("/recipes")]
    public ActionResult Index()
    {
      List<Recipe> allRecipes = Recipe.GetAll();
      return View(allRecipes);
    }

    [HttpGet("/recipes/new")]
    public ActionResult New()
    {
      return View();
    }

    [HttpPost("/recipes")]
    public ActionResult Create(string food, string category, string instructions)
    {
      Recipe newRecipe = new Recipe(food, category, instructions);
      newRecipe.Save();
      List<Recipe> allRecipes = Recipe.GetAll();

      return View("Index", allRecipes);
    }

    [HttpGet("/recipes/{id}")]
    public ActionResult Show(int id)
    {
      Dictionary<string, object> model = new Dictionary<string, object>();
      Recipe selectedRecipes = Recipe.Find(id);
      List<Ingredient> recipeIngredients = selectedRecipes.GetIngredients();
      List<Ingredient> allIngredients = Ingredient.GetAll();
      model.Add("recipe", selectedRecipes);
      model.Add("Ingredients", recipeIngredients);
      model.Add("allIngredients", allIngredients);
      return View(model);
    }



  }
}
