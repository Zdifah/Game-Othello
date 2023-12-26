# Game Othello
Program ini bangun atas dasar [Class Diagram Othello](https://www.mermaidchart.com/raw/08ca9275-0182-47ad-ae3a-747a0eb8c793?theme=light&version=v0.1&format=svg).
terdapat perubahaan dari class diagram yang lama yaitu
1. Class MoveInfo dihapus karna tidak digunakan dalam class gamecontroller
2. field const Rows and Cols di ubah jadi properti +get -set karna untuk bisa memenuhi fleksibilitas mengatur ukuran paparan
3. private Dictionary<IPlayer,Disc> _Player di ubah menjadi Dictionary<Disc, IPlayer> Players[+get -set], perubahan ini bertujuan untuk Disc menjadi key karna untuk menghidari player yang berbeda namun memilih warna yang sama. field ini menjadi properti karna untuk memberikan akses ke user untuk mengetahui daftar pemain dan warna apa saja yang dipilihnya
4. Disc[,] Board [+get] diubah menjadi Disc[,] Board [+get -set] perubahan ini bertujuan untuk memberikan fleksibilitas kepada user untuk mengatur ukuran papan serta penempatan discnya
5. Dictionary<Disc int> CountPossibelMove [+get -set] ditambahkan karna untuk meyimpan informasi current turn ini disc memiliki legal move atau tidak atau bisa dibilang variable ini sebagai penyimpanan indikator game dapat berlanjut atau tidak
6. bool GameOver [+get -set] dihapus karna sudah diwakilikan oleh GameStatus GameStat [+get -set] yang berisi status game saat ini
7. private List<Position> _outflanked ditambahkan untuk menyimpan enemy disc yang dapat dimakan
8. Action<Disc Position int> OnDiscUpdate diubah parameternya menjadi Action<Disc Position List<Position> List<Position>> karna untuk keperluan UI yang membutuhkan lebih banyak informasi berupa disk warna apa? Jalan kemana? Siapa aja yang kena makan? Posisi mana aja yang LegalMove?
9. Action<List<Position>> OnPossibelMove ditambahkan karna untuk keperluan UI menampilkan posisi legal move mana saja yang akan ditampilkan
10. method SetSizeBoard(int size) ditambahkan untuk memberikan fleksibilitas kepada user untuk mengatur ukuran papan
11. method SetDiscOnBoard(Disc disc, Position position) ditambahkan bertujuan untuk memberikan fleksibilitas kepada user untuk melalukan penempatan ulang disk ketika awal game
12. method SetRemoveDiscOnBoard(Position position) ditambahkan bertujuan untuk memberikan fleksibilitas kepada user untuk menghapus penampatan disc yang salah
13. method SetIntialTurn(Disc disc) ditambahkan untuk memberikan fleksibilitas kepada user untuk siapa yang jalan duluan
14. method StatusGame() bertujuan untuk sebagai pengaman untuk game apakah game bisa dimulai atau tidak? apakah game sudah berakhir atau tidak? apakah game sedang berlangsung atau tidak?
15. method StartGame() bertujuan sebagai pengaman jika game sudah layak untuk dimulai
16. method IsMoveLegal(Disc disc, Position position, out List<Position> outflanked) diubah parameternya menjadi (Disc disc, Position position) penghapus parameter outflanked ini bertujuan untuk menghindari user memberika input parameter yang dapat merubah field gamecontroller
17. method FindLegalMove(Disc disc) diubah parameternya menjadi parameterless karna menghindari kesalahan input dari user
18. method TryMove(Position position, bool isDefault, out MoveInfo moveInfo) diubah parameternya menjadi TryMove(Position position) karna untuk menghindari kesalahan input dari user
19. method FlipDisc(List<Position> position) diubah parameternya menjadi parameterless karna parameter dari method lama bisa diwakili dengan mengakses internal field di classnya
20. method UpdateCountDisc(Disc disc, int outflanked) diubah paramternya menjadi UpdateCountDisc(int outflanked) karna warna disk dapat diakses melalui internal classnya
21. method private ChangePlayer() dihapus karna tugas ini dapat dilakukan dengan method PassTurn()
22. method OccupiedPositions() dihapus karna tidak digunakan
# Game Othello
