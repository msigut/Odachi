﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Odachi.Validation
{
	public class ValidatorTest
	{
		private ValidationOr<string> TestBusinessMethod(string foo)
		{
			var validator = new Validator();

			if (foo.Length <= 0)
				return validator.Error(nameof(foo), "Required field");

			return $"bar_{foo}";
		}

		[Fact]
		public void Validation_success_result()
		{
			var result = TestBusinessMethod("test");

			Assert.NotNull(result.Value);
			Assert.Equal("bar_test", result.Value);

			Assert.Null(result.Validation);
		}

		[Fact]
		public void Validation_error_result()
		{
			var result = TestBusinessMethod("");

			Assert.Null(result.Value);

			Assert.NotNull(result.Validation);
			Assert.Equal("Required field", result.Validation.GetError("foo"));
			Assert.Null(result.Validation.GetError("nonexistant-field"));
		}

		[Fact]
		public void Validation_or_value_is_serializable()
		{
			var valueHolder = new ValidationOr<string>("test");

			var serialized = JsonConvert.SerializeObject(valueHolder);
			var deserialized = JsonConvert.DeserializeObject<ValidationOr<string>>(serialized);

			Assert.NotNull(deserialized.Value);
			Assert.Null(deserialized.Validation);
			Assert.Equal(valueHolder.Value, deserialized.Value);
		}
	}
}
