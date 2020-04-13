using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("/api/authorcollection")]
    public class AuthorCollectionController:ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("({ids})", Name = "GetAuthorsCollection")]
        public IActionResult GetAuthorCollection([FromRoute]
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _courseLibraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authors = _mapper.Map<IEnumerable<Models.AuthorDto>>(authorEntities);

            return Ok(authors);
        }

        [HttpPost]
        public ActionResult<IEnumerable<Models.AuthorDto>> CreateAuthors(IEnumerable<Models.AuthorInputDto> authors)
        {
            var authorsEntities = _mapper.Map<IEnumerable<Entities.Author>>(authors);
            foreach (var author in authorsEntities)
            {
                _courseLibraryRepository.AddAuthor(author);
            }

            _courseLibraryRepository.Save();

            var authorCollection = _mapper.Map<IEnumerable<Models.AuthorDto>>(authorsEntities);
            var idsAsString = string.Join(",", authorCollection.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorsCollection", new { ids = idsAsString }, authorCollection);
        }
    }
}
