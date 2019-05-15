using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;

namespace RecipeBox.Models
{
  public class Ingredient
  {
    private string _ingredientName;
    private int _id;

    public Ingredient(string ingredientName, int id = 0)
    {
      _ingredientName = ingredientName;
      _id = id;
    }
    public string IngredientName { get => _ingredientName; set => _ingredientName = value ;}
    public int Id { get => _id ;}

    public static List<Ingredient> GetAll()
    {
      List<Ingredient> allIngredients = new List<Ingredient> {};
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT * FROM ingredients;";
      MySqlDataReader rdr = cmd.ExecuteReader() as MySqlDataReader;
      while(rdr.Read())
      {
        Ingredient newIngredient = new Ingredient(rdr.GetString(0), rdr.GetInt32(1));
        allIngredients.Add(newIngredient);
      }
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return allIngredients;
    }

    public static void ClearAll()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"DELETE FROM ingredients;";
      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public static Ingredient Find(int id)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM ingredients WHERE id = (@searchId);", conn);
      cmd.Parameters.AddWithValue("@searchId", id);
      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      int ingredientId = 0;
      string ingredientName = "";
      // We remove the line setting a itemCategoryId value here.
      while(rdr.Read())
      {
        ingredientId = rdr.GetInt32(0);
        ingredientName = rdr.GetString(1);
        // We no longer read the itemCategoryId here, either.
      }
      // Constructor below no longer includes a itemCategoryId parameter:
      Ingredient newIngredient = new Ingredient(ingredientName, ingredientId);
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return newIngredient;
    }

    public override bool Equals(System.Object otherIngredient)
    {
      if (!(otherIngredient is Ingredient))
      {
        return false;
      }
      else
      {
        Ingredient newIngredient = (Ingredient) otherIngredient;
        bool idEquality = this.Id == newIngredient.Id;
        bool nameEquality = this.IngredientName == newIngredient.IngredientName;
        // We no longer compare Ingredients' recipeIds here.
        return (idEquality && nameEquality);
      }
    }

    public void Save()
    {
      // Code to declare, set, and add values to a recipeId SQL parameters has also been removed.
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"INSERT INTO ingredients (ingredient) VALUES (@ingredient);", conn);
      cmd.Parameters.AddWithValue("@ingredient", this._ingredientName);
      cmd.ExecuteNonQuery();
      _id = (int) cmd.LastInsertedId;
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public void Edit(string newIngredientName)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"UPDATE ingredients SET ingredient = @newIngredient WHERE id = @searchId;", conn);
      cmd.Parameters.AddWithValue("@searchId", _id);
      cmd.Parameters.AddWithValue("@newIngredient", newIngredientName);
      cmd.ExecuteNonQuery();
      _ingredientName = newIngredientName;
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public List<Recipe> GetRecipes()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"SELECT recipe_id FROM recipes_ingredients WHERE ingredient_id = @ingredientId;", conn);
      cmd.Parameters.AddWithValue("@ingredientId", _id);
      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      List<int> recipeIds = new List<int> {};
      while(rdr.Read())
      {
        int recipeId = rdr.GetInt32(0);
        recipeIds.Add(recipeId);
      }
      rdr.Dispose();
      List<Recipe> recipes = new List<Recipe> {};
      foreach (int recipeId in recipeIds)
      {
        MySqlCommand command = new MySqlCommand(@"SELECT * FROM recipes WHERE id = @RecipeId;", conn);
        command.Parameters.AddWithValue("@RecipeId", recipeId);
        var recipeQueryRdr = command.ExecuteReader() as MySqlDataReader;
        while(recipeQueryRdr.Read())
        {

          Recipe foundRecipe = new Recipe(
          recipeQueryRdr.GetString(0),
          recipeQueryRdr.GetString(1),
          recipeQueryRdr.GetString(2),
          recipeQueryRdr.GetInt32(3)
          );
          recipes.Add(foundRecipe);
        }
        recipeQueryRdr.Dispose();
      }
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return recipes;
    }

    public void AddRecipe (Recipe newRecipe)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand(@"INSERT INTO recipes_ingredients (recipe_id, ingredient_id) VALUES (@RecipeId, @IngredientId);", conn);
      cmd.Parameters.AddWithValue("@RecipeId", newRecipe.Id);
      cmd.Parameters.AddWithValue("@IngredientId", _id);
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
      MySqlCommand cmd = new MySqlCommand(@"DELETE FROM ingredients WHERE id = @IngredientId; DELETE FROM categories_ingredients WHERE ingredient_id = @IngredientId;", conn);
      cmd.Parameters.AddWithValue("@IngredientId", this.Id);
      cmd.ExecuteNonQuery();
      if (conn != null)
      {
        conn.Close();
      }
    }
  }
}
