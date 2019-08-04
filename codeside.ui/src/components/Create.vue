<template>
  <div class="container">
    <div class="card">
      <div class="card-header">
        <h3>Konfigürasyon Ekle</h3>
      </div>
      <div class="card-body">
        <form v-on:submit.prevent="addItem">
          <div class>
            <input
              type="text"
              class="form-control"
              id="code"
              aria-describedby="code-help"
              placeholder="Kod"
              v-model="item.Name"
            />
            <small
              id="code-help"
              class="form-text text-muted"
            >Konfigürasyon key değeri. Örn: SiteName</small>
          </div>
          <div class="form-group-left">
            <input
              type="text"
              class="form-control"
              id="value"
              aria-describedby="value-help"
              placeholder="Konfigürasyon Değeri"
              v-model="item.Value"
            />
            <small
              id="value-help"
              class="form-text text-muted"
            >Konfigürasyon değeri. Örn: www.boyner.com.tr</small>
          </div>
          <div class="form-group-left">
            <input
              type="text"
              class="form-control"
              id="type"
              aria-describedby="type-help"
              placeholder="Konfigürasyon Tipi"
              v-model="item.Type"
            />
            <small id="type-help" class="form-text text-muted">Konfigürasyon tipi. Örn: string</small>
          </div>

          <div class="form-group-left">
            <input
              type="text"
              class="form-control"
              id="application-name"
              aria-describedby="application-name-help"
              placeholder="Konfigürasyon Etki Alanı"
              v-model="item.ApplicationName"
            />
            <small
              id="type-help"
              class="form-text text-muted"
            >Konfigürasyon etki alanı. Örn: SERVICE-A</small>
          </div>
          <div class="form-group-left">
            <label class="checkbox-inline">
              <input type="checkbox" id="isactive" v-model="item.IsActive" /> Aktif
            </label>
          </div>
          <div class="form-group-left">
            <input type="submit" class="btn btn-primary" value="Ekle" />
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  components: {},
  data() {
    return {
      item: {
        Name: null,
        Value: null,
        Type: null,
        ApplicationName: null,
        IsActive: false
      }
    };
  },
  methods: {
    addItem() {
      let uri = "https://localhost:5001/api/configuration";
      this.axios
        .post(uri, this.item)
        .then(response => {
          //   this.$swal("", "İşlem tamamlandı.", "success");
          this.$router.push({ name: "Index" });
        })
        .catch(function(error) {
          //   this.$swal("", "İşlem tamamlandı.", "success");
        });
    }
  }
};
</script>