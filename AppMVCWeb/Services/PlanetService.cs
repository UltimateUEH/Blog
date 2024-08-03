using AppMVCWeb.Models;

namespace AppMVCWeb.Services
{
    public class PlanetService : List<PlanetModel>
    {
        public PlanetService()
        {
            Add(new PlanetModel
            {
                Id = 1,
                Name = "Mercury",
                VnName = "Sao Thủy",
                Content = "Mercury is the smallest planet in the Solar System and the closest to the Sun."
            });

            Add(new PlanetModel
            {
                Id = 2,
                Name = "Venus",
                VnName = "Sao Kim",
                Content = "Venus is the second planet from the Sun. It is named after the Roman goddess of love and beauty."
            });

            Add(new PlanetModel
            {
                Id = 3,
                Name = "Earth",
                VnName = "Trái Đất",
                Content = "Earth is the third planet from the Sun and the only astronomical object known to harbor life."
            });

            Add(new PlanetModel
            {
                Id = 4,
                Name = "Mars",
                VnName = "Sao Hỏa",
                Content = "Mars is the fourth planet from the Sun and the second-smallest planet in the Solar System."
            });

            Add(new PlanetModel
            {
                Id = 5,
                Name = "Jupiter",
                VnName = "Sao Mộc",
                Content = "Jupiter is the fifth planet from the Sun and the largest in the Solar System."
            });

            Add(new PlanetModel
            {
                Id = 6,
                Name = "Saturn",
                VnName = "Sao Thổ",
                Content = "Saturn is the sixth planet from the Sun and the second-largest in the Solar System."
            });

            Add(new PlanetModel
			{
				Id = 7,
				Name = "Uranus",
				VnName = "Sao Thiên Vương",
				Content = "Uranus is the seventh planet from the Sun. It has the third-largest planetary radius and fourth-largest planetary mass in the Solar System."
			});

            Add(new PlanetModel
            {
                Id = 8,
                Name = "Neptune",
                VnName = "Sao Hải Vương",
                Content = "Neptune is the eighth and farthest known Solar planet from the Sun."
            });
        }
    }
}
