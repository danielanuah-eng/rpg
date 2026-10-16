using NUnit.Framework;
using DungeonBDX.Online;

namespace DungeonBDX.Tests.Online
{
    public sealed class DungeonOnlineValidationTests
    {
        [Test]
        public void UsernamesRespectProviderAlphabetAndBoundaries()
        {
            Assert.That(DungeonOnlineValidation.IsValidUsername("Ab1._@-"), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidUsername("abc"), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidUsername(new string('a', 20)), Is.True);
            foreach (var input in new[] { null, "", "ab", new string('a', 21), "ábc", "abc def", "abc\n", "abc/def" })
                Assert.That(DungeonOnlineValidation.IsValidUsername(input), Is.False, input);
        }

        [Test]
        public void RegistrationRequiresEveryPasswordCategoryWithoutTrimming()
        {
            Assert.That(DungeonOnlineValidation.IsValidSignupPassword("Abcdef1!"), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidSignupPassword("A1!" + new string('b', 27)), Is.True);
            foreach (var input in new[] { null, "", "Abcd1!", "abcdef1!", "ABCDEF1!", "Abcdefg!", "Abcdefg1", "Abcdef1 ", "Abcd1!\nxx", "A1!" + new string('b', 28) })
                Assert.That(DungeonOnlineValidation.IsValidSignupPassword(input), Is.False, input);
        }

        [Test]
        public void LoginDoesNotApplyRegistrationCompositionRulesToExistingCredentials()
        {
            Assert.That(DungeonOnlineValidation.IsValidLoginPassword("12345678"), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidSignupPassword("12345678"), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidLoginPassword(new string('x', 30)), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidLoginPassword(new string('x', 31)), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidLoginPassword(null), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidLoginPassword("1234567"), Is.False);
        }

        [Test]
        public void RoomPasswordsAreIndependentAndOptional()
        {
            Assert.That(DungeonOnlineValidation.IsValidRoomPassword(null), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidRoomPassword(""), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidRoomPassword("12345678"), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidSignupPassword("12345678"), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidRoomPassword(new string('r', 64)), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidLoginPassword(new string('r', 64)), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidRoomPassword("1234567"), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidRoomPassword(new string('r', 65)), Is.False);
        }

        [Test]
        public void RoomCodesAreDataNotAccountCredentials()
        {
            Assert.That(DungeonOnlineValidation.IsValidRoomCode(" A7B9CD "), Is.True);
            Assert.That(DungeonOnlineValidation.IsValidRoomCode("Abcdef1!"), Is.False);
            Assert.That(DungeonOnlineValidation.IsValidSignupPassword("A7B9CD"), Is.False);
            foreach (var input in new[] { null, " ", "a/b", "a b", "abc?", new string('c', 33) })
                Assert.That(DungeonOnlineValidation.IsValidRoomCode(input), Is.False, input);
        }

        [Test]
        public void EditorTestsStillRequireRealAuthorizationAndOwnProfile()
        {
            Assert.That(DungeonOnlineValidation.CanUseOnline(false, "dungeonbdx"), Is.False);
            Assert.That(DungeonOnlineValidation.CanUseOnline(true, "arena"), Is.False);
            Assert.That(DungeonOnlineValidation.CanUseOnline(true, null), Is.False);
            Assert.That(DungeonOnlineValidation.CanUseOnline(true, "DUNGEONBDX"), Is.False);
            Assert.That(DungeonOnlineValidation.CanUseOnline(true, "dungeonbdx"), Is.True);
        }
    }
}
