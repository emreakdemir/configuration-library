<template>
  <div class="container">
    <div class="card">
      <div class="card-header">
        <h3>Konfigürasyon Düzenle</h3>
      </div>
      <div class="card-body">
        <form v-on:submit.prevent="updateItem">
          <div class>
            <input
              type="text"
              class="form-control"
              id="code"
              aria-describedby="code-help"
              placeholder="Kod"
              v-model="item.name"
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
              v-model="item.value"
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
              v-model="item.type"
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
              v-model="item.applicationName"
            />
            <small
              id="type-help"
              class="form-text text-muted"
            >Konfigürasyon etki alanı. Örn: SERVICE-A</small>
          </div>
          <div class="form-group-left">
            <label class="checkbox-inline">
              <input type="checkbox" id="value" v-model="item.isActive" /> Aktif
            </label>
          </div>
          <div class="form-group-left">
            <input type="submit" class="btn btn-primary" value="Kaydet" />
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
<script>
import { debuglog } from "util";
export default {
  data() {
    return {
      item: {}
    };
  },
  created: function() {
    this.getItem();
  },
  methods: {
    getItem() {
      var self = this;
      let uri =
        "https://localhost:5001/api/configuration/" + this.$route.params.id;
      this.axios.get(uri).then(response => {
        self.item = response.data;
      });
    },

    updateItem() {
      var self = this;
      let uri =
        "https://localhost:5001/api/configuration/" + this.$route.params.id;
      this.axios
        .put(uri, {
          Id: this.$route.params.id,
          Name: this.item.name,
          Value: this.item.value,
          Type: this.item.type,
          ApplicationName: this.item.applicationName,
          IsActive: this.item.isActive
        })
        .then(response => {
          self.$router.push({ name: "Index" });
        });
    }
  }
};
</script>