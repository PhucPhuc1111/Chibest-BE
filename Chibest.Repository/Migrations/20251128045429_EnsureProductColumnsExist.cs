using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chibest.Repository.Migrations
{
    /// <inheritdoc />
    public partial class EnsureProductColumnsExist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure all Product columns exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Product') THEN
                        -- Add Material column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'Material') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""Material"" VARCHAR(100) NULL;
                        END IF;
                        
                        -- Add Style column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'Style') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""Style"" VARCHAR(100) NULL;
                        END IF;
                        
                        -- Add Weight column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'Weight') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""Weight"" INTEGER NOT NULL DEFAULT 0;
                        END IF;
                        
                        -- Add BarCode column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'BarCode') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""BarCode"" VARCHAR(100) NULL;
                        END IF;
                        
                        -- Add IsMaster column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'IsMaster') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""IsMaster"" BOOLEAN NOT NULL DEFAULT TRUE;
                        END IF;
                        
                        -- Add Note column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'Note') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""Note"" VARCHAR(200) NULL;
                        END IF;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
