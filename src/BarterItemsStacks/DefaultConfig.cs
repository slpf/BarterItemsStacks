namespace BarterItemsStacks;

public static class DefaultConfig
{
    private const string Config = """
{
	"Items": {
		// Cash
		"5449016a4bdc2d6f028b456f": { "StackSize": 20000000 },	// Roubles
		"5696686a4bdc2da3298b456a": { "StackSize": 20000000 },	// Dollars
		"569668774bdc2da2298b4568": { "StackSize": 20000000 },	// Euros
		"5d235b4d86f7742e017bc88a": { "StackSize": 2000 },		// GP coin
		"6656560053eaaa7a23349c86": { "StackSize": 20 },		// Lega Medal
		"59faff1d86f7746c51718c9c": { "StackSize": 20 },		// Physical Bitcoin

		// Barter items
		"5bc9b355d4351e6d1509862a": { "StackSize": 5 },		// #FireKlean gun lube
		"5bc9be8fd4351e00334cae6e": { "StackSize": 5 },		// 42 Signature Blend English Tea
		"5d03794386f77420415576f5": { "StackSize": 5 },		// 6-STEN-140-M military battery
		"5672cb124bdc2d1a0f8b4568": { "StackSize": 20 },	// AA Battery
		"6389c85357baa773a825b356": { "StackSize": 5 },		// Advanced current converter
		"59faf98186f774067b6be103": { "StackSize": 10 },	// Alkaline cleaner for heat exchangers
		"5d1b32c186f774252167a530": { "StackSize": 5 },		// Analog thermometer
		"590de71386f774347051a052": { "StackSize": 5 },		// Antique teapot
		"590de7e986f7741b096e5f32": { "StackSize": 5 },		// Antique vase
		"573475fb24597737fb1379e1": { "StackSize": 20 },	// Apollo Soyuz cigarettes
		"6389c6c7dbfd5e4b95197e68": { "StackSize": 10 },	// Aquapeps water purification tablets
		"5e2af4d286f7746d4159f07a": { "StackSize": 10 },	// Aramid fiber fabric
		"62a0a098de7ac8199358053b": { "StackSize": 10 },	// Awl
		"62a091170b9d3c46de5b6cf2": { "StackSize": 5 },		// Axel parrot figurine
		"5bc9c049d4351e44f824d360": { "StackSize": 5 },		// Battered antique book
		"62a08f4c4f842e1bd12d9d62": { "StackSize": 5 },		// BEAR Buddy plush toy
		"655c652d60d0ac437100fed7": { "StackSize": 5 },		// BEAR operative figurine
		"57347c5b245977448d35f6e1": { "StackSize": 20 },	// Bolts
		"59e361e886f774176c10a2a5": { "StackSize": 10 },	// Bottle of hydrogen peroxide
		"62a0a043cf4a99369e2624a5": { "StackSize": 10 },	// Bottle of OLOLO Multivitamins
		"59e3606886f77417674759a5": { "StackSize": 10 },	// Bottle of saline solution
		"56742c324bdc2d150f8b456d": { "StackSize": 10 },	// Broken GPhone smartphone
		"5c1265fc86f7743f896a21c2": { "StackSize": 10 },	// Broken GPhone X smartphone
		"5d1b309586f77425227d1676": { "StackSize": 10 },	// Broken LCD
		"59e3639286f7741777737013": { "StackSize": 5 },		// Bronze lion figurine
		"619cbfeb6b8a1b37a54eebfa": { "StackSize": 5 },		// Bulbex cable cutter
		"5c06779c86f77426e00dd782": { "StackSize": 20 },	// Bundle of wires
		"5e54f6af86f7742199090bf3": { "StackSize": 5 },		// Can of Dr. Lupo's coffee beans
		"5af0484c86f7740f02001f7f": { "StackSize": 10 },	// Can of Majaica coffee beans
		"60391a8b3364dc22b04d0ce5": { "StackSize": 5 },		// Can of thermite
		"62a09ee4cf4a99369e262453": { "StackSize": 20 },	// Can of white salt
		"5c06782b86f77426df5407d2": { "StackSize": 20 },	// Capacitors
		"5733279d245977289b77ec24": { "StackSize": 10 },	// Car battery
		"59e3658a86f7741776641ac4": { "StackSize": 10 },	// Cat figurine
		"5c1267ee86f77416ec610f72": { "StackSize": 5 },		// Chain with Prokill medallion
		"573474f924597738002c6174": { "StackSize": 20 },	// Chainlet
		"57347b8b24597737dd42e192": { "StackSize": 20 },	// Classic matches
		"59e358a886f7741776641ac3": { "StackSize": 20 },	// Clin window cleaner
		"590c2c9c86f774245b1f03f2": { "StackSize": 20 },	// Construction measuring tape
		"5e2af41e86f774755a234b67": { "StackSize": 10 },	// Cordura polyamide fabric
		"59e35cbb86f7741778269d83": { "StackSize": 20 },	// Corrugated hose
		"679b9d4b3374fb14f40efe6d": { "StackSize": 5 },		// Count Bloodsucker figurine
		"5734779624597737e04bf329": { "StackSize": 20 },	// CPU fan
		"56742c284bdc2d98058b456d": { "StackSize": 20 },	// Crickent lighter
		"655c669103999d3c810c025b": { "StackSize": 5 },		// Cultist figurine
		"5e2aee0a86f774755a234b62": { "StackSize": 5 },		// Cyclon rechargeable battery
		"5672cb304bdc2dc2088b456a": { "StackSize": 20 },	// D Size battery
		"590a386e86f77429692b27ab": { "StackSize": 20 },	// Damaged hard drive
		"5bc9b9ecd4351e3bac122519": { "StackSize": 5 },		// Deadlyslob's beard oil
		"655c67ab0d37ca5135388f4b": { "StackSize": 5 },		// Ded Moroz figurine
		"66572b8d80b1cd4b6a67847f": { "StackSize": 5 },		// Den figurine
		"5d1b3f2d86f774253763b735": { "StackSize": 20 },	// Disposable syringe
		"590a373286f774287540368b": { "StackSize": 5 },		// Dry fuel
		"57347c1124597737fb1379e3": { "StackSize": 20 },	// Duct tape
		"5734781f24597737e04bf32a": { "StackSize": 20 },	// DVD drive
		"59e35de086f7741778269d84": { "StackSize": 10 },	// Electric drill
		"5d1b2fa286f77425227d1674": { "StackSize": 10 },	// Electric motor
		"6389c70ca33d8c4cdf4932c6": { "StackSize": 10 },	// Electronic components
		"590a3cd386f77436f20848cb": { "StackSize": 10 },	// Energy-saving lamp
		"5d1b371186f774253763a656": { "StackSize": 5 },		// Expeditionary fuel tank
		"6389c7f115805221fb410466": { "StackSize": 5 },		// Far-forward GPS Signal Amplifier Unit
		"63a0b208f444d32d6f03ea1e": { "StackSize": 10 },	// Fierce Blow sledgehammer
		"5d63d33b86f7746ea9275524": { "StackSize": 20 },	// Flat screwdriver
		"5d4042a986f7743185265463": { "StackSize": 10 },	// Flat screwdriver (Long)
		"5e2af47786f7746d404f3aaa": { "StackSize": 10 },	// Fleece fabric
		"5d1b2f3f86f774252167a52c": { "StackSize": 5 },		// FP-100 filter absorber
		"5b43575a86f77424f443fe62": { "StackSize": 5 },		// Fuel conditioner
		"590a3efd86f77437d351a25b": { "StackSize": 10 },	// Gas analyzer
		"590c595c86f7747884343ad7": { "StackSize": 10 },	// Gas mask air filter
		"5672cb724bdc2dc2088b456b": { "StackSize": 10 },	// Geiger-Muller counter
		"5d235a5986f77443f6329bc6": { "StackSize": 5 },		// Gold skull ring
		"5bc9b720d4351e450201234b": { "StackSize": 5 },		// Golden 1GPhone smartphone
		"62a09cfe4f842e1bd12da3e4": { "StackSize": 5 },		// Golden egg
		"5734758f24597738025ee253": { "StackSize": 10 },	// Golden neck chain
		"5bc9bc53d4351e00367fbcee": { "StackSize": 5 },		// Golden rooster figurine
		"57347ca924597744596b4e71": { "StackSize": 5 },		// Graphics card
		"5e2aedd986f7746d404f3aa4": { "StackSize": 10 },	// GreenBat lithium battery
		"5d6fc78386f77449d825f9dc": { "StackSize": 10 },	// Gunpowder "Eagle"
		"5d6fc87386f77449db3db94e": { "StackSize": 5 },		// Gunpowder "Hawk"
		"590c5a7286f7747884343aea": { "StackSize": 20 },	// Gunpowder "Kite"
		"5d1b317c86f7742523398392": { "StackSize": 10 },	// Hand drill
		"573478bc24597738002c6175": { "StackSize": 20 },	// Horse figurine
		"5e2af2bc86f7746d3f3c33fc": { "StackSize": 20 },	// Hunting matches
		"66b37f114410565a8f6789e2": { "StackSize": 5 },		// Inseq gas pipe wrench
		"5734795124597738002c6176": { "StackSize": 20 },	// Insulating tape
		"5d0377ce86f774186372f689": { "StackSize": 5 },		// Iridium military thermal vision module
		"5e2af29386f7746d4159f077": { "StackSize": 10 },	// KEKTAPE duct tape
		"66572c82ad599021091c6118": { "StackSize": 5 },		// Killa figurine
		"5c0530ee86f774697952d952": { "StackSize": 5 },		// LEDX Skin Transilluminator
		"5d1b392c86f77425243e98fe": { "StackSize": 20 },	// Light bulb
		"60b0f7057897d47c5b04ab94": { "StackSize": 5 },		// Loot Lord plushie
		"60b0f561c4449e4cb624c1d7": { "StackSize": 5 },		// LVNDMARK's rat poison
		"590a391c86f774385a33c404": { "StackSize": 10 },	// Magnet
		"573476d324597737da2adc13": { "StackSize": 20 },	// Malboro Cigarettes
		"5b4335ba86f7744d2837a264": { "StackSize": 10 },	// Medical bloodset
		"619cc01e0a7c3a1a2731940c": { "StackSize": 10 },	// Medical tools
		"5d40419286f774318526545f": { "StackSize": 10 },	// Metal cutting scissors
		"5d1b36a186f7742523398433": { "StackSize": 5 },		// Metal fuel tank
		"61bf7b6302b3924be92fa8c3": { "StackSize": 20 },	// Metal spare parts
		"6389c7750ef44505c87f5996": { "StackSize": 5 },		// Microcontroller board
		"5d0375ff86f774186372f685": { "StackSize": 5 },		// Military cable
		"5d0376a486f7747d8050965c": { "StackSize": 5 },		// Military circuit board
		"5c052f6886f7746b1e3db148": { "StackSize": 5 },		// Military COFDM Wireless Signal Transmitter
		"619cbf476b8a1b37a54eebf8": { "StackSize": 10 },	// Military corrugated tube
		"5d03784a86f774203e7e0c4d": { "StackSize": 5 },		// Military gyrotachometer
		"5d0378d486f77420421a5ff4": { "StackSize": 5 },		// Military power filter
		"679b9cce4e4ed4b3b40ae5c5": { "StackSize": 5 },		// Nailhead figurine
		"5d40425986f7743185265461": { "StackSize": 20 },	// Nippers
		"5d1b2ffd86f77425243e8d17": { "StackSize": 5 },		// NIXXOR lens
		"5d0379a886f77420407aa271": { "StackSize": 5 },		// OFZ 30x165mm shell
		"5bc9c377d4351e3bac12251b": { "StackSize": 5 },		// Old firesteel
		"5af0534a86f7743b6f354284": { "StackSize": 5 },		// Ophthalmoscope
		"5d4041f086f7743cac3f22a7": { "StackSize": 20 },	// Ortodontox toothpaste
		"59e3556c86f7741776641ac2": { "StackSize": 20 },	// Ox bleach
		"6389c6463485cf0eeb260715": { "StackSize": 10 },	// Pack of Arseniy buckwheat
		"5e2af02c86f7746d420957d4": { "StackSize": 10 },	// Pack of chlorine
		"590c31c586f774245e3141b2": { "StackSize": 20 },	// Pack of nails
		"59e35ef086f7741777737012": { "StackSize": 20 },	// Pack of screws
		"59e35abd86f7741778269d82": { "StackSize": 20 },	// Pack of sodium bicarbonate
		"59e3596386f774176c10a2a2": { "StackSize": 20 },	// PAID AntiRoach spray
		"5c12688486f77426843c7d32": { "StackSize": 10 },	// Paracord
		"573477e124597737dd42e191": { "StackSize": 10 },	// PC CPU
		"679b9d43597ba2ed120c3d44": { "StackSize": 5 },		// Petya Crooker figurine
		"5d1b313086f77425227d1678": { "StackSize": 20 },	// Phase control relay
		"5d03775b86f774203e7e0c4b": { "StackSize": 5 },		// Phased array element
		"59e366c186f7741778269d85": { "StackSize": 10 },	// Piece of plexiglass
		"5d1b3a5d86f774252167ba22": { "StackSize": 10 },	// Pile of meds
		"619cbfccbedcde2f5b3f7bdd": { "StackSize": 5 },		// Pipe grip wrench
		"590c2b4386f77425357b6123": { "StackSize": 20 },	// Pliers
		"5af04b6486f774195a3ebb49": { "StackSize": 10 },	// Pliers Elite
		"6841b6b674a3c16f5e03d653": { "StackSize": 5 },		// PMC Origins figurine
		"679b9d6390622daf9708da76": { "StackSize": 5 },		// Pointy guy figurine
		"655c66e40b2de553b618d4b8": { "StackSize": 5 },		// Politician Mutkevich figurine
		"5c052e6986f7746b207bc3c9": { "StackSize": 5 },		// Portable defibrillator
		"5af0561e86f7745f5f3ad6ac": { "StackSize": 10 },	// Portable Powerbank
		"59e36c6f86f774176c10a2a7": { "StackSize": 10 },	// Power cord
		"57347c2e24597744902c94a1": { "StackSize": 10 },	// Power supply unit
		"62a09cb7a04c0c5c6e0a84f8": { "StackSize": 5 },		// Press pass (issued for NoiceGuy)
		"5d1b327086f7742525194449": { "StackSize": 5 },		// Pressure gauge
		"590a3b0486f7743954552bdb": { "StackSize": 10 },	// Printed circuit board
		"577e1c9d2459773cd707c525": { "StackSize": 20 },	// Printer paper
		"59fafb5d86f774067a6f2084": { "StackSize": 10 },	// Propane tank (5L)
		"5d1c774f86f7746d6620f8db": { "StackSize": 10 },	// Radiator helix
		"57347baf24597738002c6178": { "StackSize": 10 },	// RAM stick
		"60391afc25aff57af81f7085": { "StackSize": 5 },		// Ratchet wrench
		"5e54f62086f774219b0f1937": { "StackSize": 5 },		// Raven figurine
		"590a358486f77429692b2790": { "StackSize": 10 },	// Rechargeable battery
		"5e2aef7986f7746d3f3c33f5": { "StackSize": 20 },	// Repellent
		"66572be36a723f7f005a066e": { "StackSize": 5 },		// Reshala figurine
		"5e2af4a786f7746d3f3c3400": { "StackSize": 10 },	// Ripstop fabric
		"59faf7ca86f7740dbe19f6c2": { "StackSize": 5 },		// Roler Submariner gold wrist watch
		"5d1b31ce86f7742523398394": { "StackSize": 10 },	// Round pliers
		"655c67782a1356436041c9c5": { "StackSize": 5 },		// Ryzhy figurine
		"655c673673a43e23e857aebd": { "StackSize": 5 },		// Scav figurine
		"5d40412b86f7743cb332ac3a": { "StackSize": 20 },	// Schaman shampoo
		"57347c77245977448d35f6e2": { "StackSize": 20 },	// Screw nuts
		"590c2d8786f774245b1f03f3": { "StackSize": 20 },	// Screwdriver
		"62a0a0bb621468534a797ad5": { "StackSize": 10 },	// Set of files "Master"
		"61bf83814088ec1a363d7097": { "StackSize": 10 },	// Sewing kit
		"590c35a486f774273531c822": { "StackSize": 10 },	// Shustrilo sealing foam
		"5d1b39a386f774252339976f": { "StackSize": 10 },	// Silicone tube
		"5bc9bdb8d4351e003562b8a1": { "StackSize": 5 },		// Silver Badge
		"5e2af00086f7746d3f3c33f7": { "StackSize": 20 },	// Smoked Chimney drain cleaner
		"5c13cd2486f774072c757944": { "StackSize": 20 },	// Soap
		"590a3c0a86f774385a33c450": { "StackSize": 10 },	// Spark plug
		"5734770f24597738025ee254": { "StackSize": 20 },	// Strike Cigarettes
		"5e2af37686f774755a234b65": { "StackSize": 10 },	// SurvL Survivor Lighter
		"57347cd0245977445a2d6ff1": { "StackSize": 20 },	// T-Shaped plug
		"66572cbdad599021091c611a": { "StackSize": 5 },		// Tagilla figurine
		"66b37ea4c5d72b0277488439": { "StackSize": 5 },		// Tamatthi kunai knife replica
		"5c12620d86f7743f8b198b72": { "StackSize": 5 },		// Tetriz portable game console
		"5c13cef886f774072e618e82": { "StackSize": 20 },	// Toilet paper
		"590c2e1186f77425357b6124": { "StackSize": 5 },		// Toolset
		"57347c93245977448d35f6e3": { "StackSize": 20 },	// Toothpaste
		"60391b0fb847c71012789415": { "StackSize": 10 },	// TP-200 TNT brick
		"5e2af22086f7746d3f3c33fa": { "StackSize": 10 },	// Tube of Poxeram cold welding
		"5c052fb986f7746b2101e909": { "StackSize": 5 },		// UHF RFID Reader
		"590a3d9c86f774385926e510": { "StackSize": 10 },	// Ultraviolet lamp
		"5909e99886f7740c983b9984": { "StackSize": 20 },	// USB Adapter
		"655c663a6689c676ce57af85": { "StackSize": 5 },		// USEC operative figurine
		"5e2af51086f7746d3f3c3402": { "StackSize": 20 },	// UZRGM grenade fuze
		"5f745ee30acaeb0d490d8c5b": { "StackSize": 5 },		// Veritas guitar pick
		"66b37eb4acff495a29492407": { "StackSize": 5 },		// Viibiin sneaker
		"5c05308086f7746b2101e90b": { "StackSize": 5 },		// Virtex programmable processor
		"5c05300686f7746dce784e5d": { "StackSize": 5 },		// VPX Flash Storage Module
		"5d1b385e86f774252167b98a": { "StackSize": 10 },	// Water filter
		"590c5bbd86f774785762df04": { "StackSize": 10 },	// WD-40 (100ml)
		"590c5c9f86f77477c91c36e7": { "StackSize": 10 },	// WD-40 (400ml)
		"5d1c819a86f774771b0acd6c": { "StackSize": 10 },	// Weapon parts
		"573476f124597737e04bf328": { "StackSize": 20 },	// Wilston cigarettes
		"59e3647686f774176a362507": { "StackSize": 5 },		// Wooden clock
		"5d1b304286f774253763a528": { "StackSize": 10 },	// Working LCD
		"590c311186f77424d1667482": { "StackSize": 20 },	// Wrench
		"679b9d55708cfcb2060b9ae3": { "StackSize": 5 },		// Xenoalien figurine
		"590c346786f77423e50ed342": { "StackSize": 10 },	// Xenomorph sealing foam
		"56742c2e4bdc2d95058b456d": { "StackSize": 20 },	// Zibbo lighter

		// Spec items
		"67ab3d4b83869afd170fdd3f": { "StackSize": 5 },		// BBQ-S43 gas torch
		"5f4f9eb969cdc30ff33f09db": { "StackSize": 5 },		// EYE MK.2 professional hand-held compass
		"544fb5454bdc2df8738b456a": { "StackSize": 5 },		// Leatherman Multitool
		"5991b51486f77447b112d44f": { "StackSize": 5 },		// MS2000 Marker
		"63a0b2eabea67a6d93009e52": { "StackSize": 5 },		// Radio repeater
		"5ac78a9b86f7741cca0bbd8d": { "StackSize": 5 },		// Signal Jammer
		"666b11055a706400b717cfa5": { "StackSize": 5 },		// Tripwire installation kit
		"678fa929819ddc4c350c0317": { "StackSize": 5 },		// Valve handwheel
		"61605e13ffa6e502ac5e7eef": { "StackSize": 5 },		// Vortex Ranger 1500 rangefinder
		"5b4391a586f7745321235ab2": { "StackSize": 5 },		// WI-FI Camera

		// Info items
		"6389c92d52123d5dd17f8876": { "StackSize": 5 },		// Advanced Electronic Materials textbook
		"62a09e73af34e73a266d932a": { "StackSize": 5 },		// BakeEzy cook book
		"590c645c86f77412b01304d9": { "StackSize": 5 },		// Diary
		"660bbc47c38b837877075e47": { "StackSize": 5 },		// Encrypted flash drive
		"5c12613b86f7743bbe2c3f76": { "StackSize": 5 },		// Intelligence folder
		"679b992329acd1f2f60985a5": { "StackSize": 5 },		// Labrys research notes
		"62a0a16d0b9d3c46de5b6e97": { "StackSize": 5 },		// Military flash drive
		"590c37d286f77443be3d7827": { "StackSize": 5 },		// SAS drive
		"590c621186f774138d11ea29": { "StackSize": 5 },		// Secure Flash drive
		"61bf7c024770ee6f9c6b8b53": { "StackSize": 5 },		// Secure magnetic tape cassette
		"6389c8fb46b54c634724d847": { "StackSize": 3 },		// Silicon Optoelectronic Integrated Circuits textbook
		"590c651286f7741e566b6461": { "StackSize": 5 },		// Slim diary
		"590c392f86f77444754deb29": { "StackSize": 5 },		// SSD drive
		"590c639286f774151567fa95": { "StackSize": 5 },		// Tech manual
		"6389c8c5dbfd5e4b95197e6b": { "StackSize": 5 },		// TerraGroup "Blue Folders" materials
		"6889e4cd3ff7e9cfbc0c507c": { "StackSize": 5 },		// Thumb drive with military data
		"62a0a124de7ac81993580542": { "StackSize": 5 },		// Topographic survey maps
		"62a09e974f842e1bd12da3f0": { "StackSize": 5 }		// Video cassette with the Cyborg Killer movie
	}
}
""";
    
    public static void Create(string configPath)
    {
	    File.WriteAllText(configPath, Config);
    }
}