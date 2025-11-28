using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chibest.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure Product table has all required columns
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
                        
                        -- Add CreatedAt column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'CreatedAt') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""CreatedAt"" TIMESTAMP(3) WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP;
                        END IF;
                        
                        -- Add UpdatedAt column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'UpdatedAt') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""UpdatedAt"" TIMESTAMP(3) WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP;
                        END IF;
                        
                        -- Add ColorId column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'ColorId') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""ColorId"" UUID NULL;
                        END IF;
                        
                        -- Add SizeId column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'SizeId') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""SizeId"" UUID NULL;
                        END IF;
                        
                        -- Add SupplierId column if it doesn't exist
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Product' AND column_name = 'SupplierId') THEN
                            ALTER TABLE ""Product"" ADD COLUMN ""SupplierId"" UUID NULL;
                        END IF;
                        
                        -- Create indexes if they don't exist
                        IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'Product' AND indexname = 'ix_product_colorid') THEN
                            CREATE INDEX ""ix_product_colorid"" ON ""Product"" (""ColorId"");
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'Product' AND indexname = 'ix_product_sizeid') THEN
                            CREATE INDEX ""ix_product_sizeid"" ON ""Product"" (""SizeId"");
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'Product' AND indexname = 'ix_product_supplierid') THEN
                            CREATE INDEX ""ix_product_supplierid"" ON ""Product"" (""SupplierId"");
                        END IF;
                        
                        -- Add foreign keys if they don't exist
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.table_constraints 
                            WHERE constraint_schema = 'public' 
                            AND table_name = 'Product' 
                            AND constraint_name = 'Product_ColorId_fkey'
                        ) AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Color') THEN
                            ALTER TABLE ""Product"" 
                            ADD CONSTRAINT ""Product_ColorId_fkey"" 
                            FOREIGN KEY (""ColorId"") 
                            REFERENCES ""Color"" (""Id"") 
                            ON DELETE SET NULL;
                        END IF;
                        
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.table_constraints 
                            WHERE constraint_schema = 'public' 
                            AND table_name = 'Product' 
                            AND constraint_name = 'Product_SizeId_fkey'
                        ) AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Size') THEN
                            ALTER TABLE ""Product"" 
                            ADD CONSTRAINT ""Product_SizeId_fkey"" 
                            FOREIGN KEY (""SizeId"") 
                            REFERENCES ""Size"" (""Id"") 
                            ON DELETE SET NULL;
                        END IF;
                        
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.table_constraints 
                            WHERE constraint_schema = 'public' 
                            AND table_name = 'Product' 
                            AND constraint_name = 'Product_SupplierId_fkey'
                        ) AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Account') THEN
                            ALTER TABLE ""Product"" 
                            ADD CONSTRAINT ""Product_SupplierId_fkey"" 
                            FOREIGN KEY (""SupplierId"") 
                            REFERENCES ""Account"" (""Id"") 
                            ON DELETE SET NULL;
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
