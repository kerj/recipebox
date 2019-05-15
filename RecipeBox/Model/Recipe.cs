using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;

namespace RecipeBox.Models
{
  public class Recipe
  {

    private string _food;
    private string _category;
    private string _instruction;
    private int _id;

    public Recipe(string food, string category, string instructions, int id = 0)
    {
      _food = food;
      _category = category;
      _instruction = instructions;
      _id = id;
    }

    public string Food { get => _food; set => _food = value;}
    public string Category { get => _category; set => _category = value;}
    public string Instruction { get => _instruction; set => _instruction = value;}
    public int Id { get => _id;}

    public static List<Recipe> GetAll()
    {
      List<Recipe> allRecipes = new List<Recipe> {};
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT * FROM recipes;";
      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      while(rdr.Read())
      {
        int RecipeId = rdr.GetInt32(0);
        string FoodName = rdr.GetString(1);
        string RecipeInstructions = rdr.GetString(2);
        string CategoryName = rdr.GetString(3);
        Recipe newRecipe = new Recipe(FoodName, RecipeInstructions, CategoryName, RecipeId);

        allRecipes.Add(newRecipe);

      }
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return allRecipes;
    }

    public static void ClearAll()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"DELETE FROM recipes;";
      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public static Recipe Find(int id)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM recipes WHERE id = (@searchId);", conn);
      cmd.Parameters.AddWithValue("@searchId", id);
      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      int RecipeId = 0;
      string FoodName = "";
      string RecipeInstructions = "";
      string CategoryName = "";
      // We remove the line setting a ingredientRecipeId value here.
      while(rdr.Read())
      {
        RecipeId = rdr.GetInt32(0);
        FoodName = rdr.GetString(1);
        RecipeInstructions = rdr.GetString(2);
        CategoryName = rdr.GetString(3);
        // We no longer read the patientRecipeId here, either.
      }

      Recipe newRecipe = new Recipe(FoodName, RecipeInstructions, CategoryName, RecipeId);

      // Constructor below no longer includes a patientRecipeId parameter:


      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return newRecipe;
    }

    public List<Ingredient> GetIngredients()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();

      MySqlCommand cmd = new MySqlCommand(@"SELECT ingredient_id FROM recipes_ingredients WHERE recipe_id = @RecipeId;", conn);
      cmd.Parameters.AddWithValue("@RecipeId", _id);
      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      List<int> ingredientIds = new List<int> {};
      while(rdr.Read())
      {
        int ingredientId = rdr.GetInt32(0);
        ingredientIds.Add(ingredientId);
      }
      rdr.Dispose();
      List<Ingredient> ingredients = new List<Ingredient> {};
      foreach (int ingredientId in ingredientIds)
      {
        var ingredientQuery = conn.CreateCommand() as MySqlCommand;
        ingredientQuery.CommandText = @"SELECT * FROM ingredients WHERE id = @IngredientId;";
        MySqlParameter ingredientIdParameter = new MySqlParameter();
        ingredientIdParameter.ParameterName = "@IngredientId";
        ingredientIdParameter.Value = ingredientId;
        ingredientQuery.Parameters.Add(ingredientIdParameter);
        var ingredientQueryRdr = ingredientQuery.ExecuteReader() as MySqlDataReader;
        while(ingredientQueryRdr.Read())
        {
          int thisIngredientId = ingredientQueryRdr.GetInt32(0);
          string ingredientDescription = ingredientQueryRdr.GetString(1);
          Ingredient foundIngredient = new Ingredient (ingredientDescription, thisIngredientId);
          ingredients.Add(foundIngredient);
        }
        ingredientQueryRdr.Dispose();
      }
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return ingredients;
    }

    public override bool Equals(System.Object otherRecipe)
    {
      if (!(otherRecipe is Recipe))
      {
        return false;
      }
      else
      {
        Recipe newRecipe = (Recipe) otherRecipe;
        bool idEquality = this.Id.Equals(newRecipe.Id);
        bool foodEquality = this.Food.Equals(newRecipe.Food);
        bool instructionsEquality = this.Instruction.Equals(newRecipe.Instruction);
        bool categoryEquality = this.Category.Equals(newRecipe.Category);
        return (idEquality &&
              foodEquality &&
              instructionsEquality &&
              categoryEquality
              );
      }
    }

    public void Save()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"INSERT INTO recipes (food, instructions, category) VALUES (@food, @instructions, @category);", conn);
      cmd.Parameters.AddWithValue("@food", this._food);
      cmd.Parameters.AddWithValue("@instructions", this._instruction);
      cmd.Parameters.AddWithValue("@category", this._category);

      cmd.ExecuteNonQuery();
      _id = (int) cmd.LastInsertedId; // <-- This line is new!
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public void DeleteRecipe(int recipeId)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;

      Recipe selectedRecipe = Recipe.Find(recipeId);
      Dictionary<string, object> model = new Dictionary<string, object>();
      List<Ingredient> recipeIngredient = selectedRecipe.GetIngredients();
      model.Add("recipe", selectedRecipe);

      foreach (Ingredient ingredient in recipeIngredient)
      {
        ingredient.Delete();
      }

      cmd.CommandText = @"DELETE FROM recipes WHERE id = @thisId;";
      cmd.Parameters.AddWithValue("@thisId", _id);
      cmd.ExecuteNonQuery();

      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }
    public void Delete()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand("DELETE FROM recipes WHERE id = @RecipeId; DELETE FROM recipes_ingredients WHERE recipe_id = @RecipeId;", conn);
      cmd.Parameters.AddWithValue("@RecipeId", this.Id);
      cmd.ExecuteNonQuery();

      if (conn != null)
      {
        conn.Close();
      }
    }

  }
}
