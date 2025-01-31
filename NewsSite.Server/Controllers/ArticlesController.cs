using Microsoft.AspNetCore.Mvc;
using NewsSite.Server.Interfaces;
using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using NewsSite.Server.Models.Common;
using NewsSite.Server.Services.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topic = NewsSite.Server.Models;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleRepository _articleRepository;
        private readonly INewsCategoryService _newsCategoryService;

        public ArticlesController(
            IArticleRepository articleRepository,
            INewsCategoryService newsCategoryService)
        {
            _articleRepository = articleRepository;
            _newsCategoryService = newsCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Article>>> GetArticles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string categoryId = null,
            [FromQuery] string topicId = null,
            [FromQuery] string language = "en")
        {
            if (!string.IsNullOrEmpty(categoryId))
            {
                var category = await _articleRepository.GetCategoryByIdAsync(categoryId);
                if (category != null)
                {
                    var articlesByCategory = await _articleRepository.GetArticlesAsync(page, pageSize, categoryId, topicId, language);
                    return Ok(articlesByCategory);
                }
            }

            var articles = await _articleRepository.GetArticlesAsync(page, pageSize, categoryId, topicId, language);
            return Ok(articles);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories([FromQuery] string language = "en")
        {
            var categories = await _articleRepository.GetCategoriesAsync(language);
            return Ok(categories);
        }

        [HttpGet("categories/{categoryId}/topics")]
        public async Task<ActionResult<IEnumerable<SocialMediaTopic>>> GetTopics(string categoryId, [FromQuery] string language = "en")
        {
            var topics = await _articleRepository.GetTopicsByCategoryAsync(categoryId, language);
            return Ok(topics);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(string id)
        {
            var article = await _articleRepository.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);
        }

        [HttpPost]
        public async Task<ActionResult<Article>> CreateArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdArticle = await _articleRepository.CreateArticleAsync(article);
            return CreatedAtAction(nameof(GetArticle), new { id = createdArticle.Id }, createdArticle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(string id, Article article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            var success = await _articleRepository.UpdateArticleAsync(article);
           

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            await _articleRepository.DeleteArticleAsync(id);
           

            return NoContent();
        }

        [HttpPost("categories")]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCategory = await _articleRepository.CreateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategories), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(string id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            var success = await _articleRepository.UpdateCategoryAsync(category);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var success = await _articleRepository.DeleteCategoryAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        //[HttpPost("categories/{categoryId}/topics")]
        //public async Task<ActionResult<Models.Topic>> CreateTopic(string categoryId, Models.Topic topic)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    topic.CategoryId = categoryId;
        //    var createdTopic = await _articleRepository.CreateTopicAsync(topic);
        //    return CreatedAtAction(nameof(GetTopics), new { categoryId, id = createdTopic.Id }, createdTopic);
        //}

        [HttpGet("saved")]
        public async Task<ActionResult<PagedResult<Article>>> GetSavedArticles(
            [FromQuery(Name = "ids[]")] List<string> ids,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (ids == null || !ids.Any())
            {
                return Ok(new PagedResult<Article>
                {
                    Items = new List<Article>(),
                    TotalItems = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }

            var articles = await _articleRepository.GetArticlesByIdsAsync(ids, page, pageSize);
            return Ok(articles);
        }
    }
} 